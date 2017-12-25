using System;
using System.Collections.Generic;
using System.Linq;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Xmu.Crms.Services.HotPot
{
    public class ClassService :IClassService
    {
        private  CrmsContext _db;

        public ClassService(CrmsContext db)
        {
            _db = db;
        }
        /// <summary>
        /// 按班级id删除班级.
        /// @author cuiheyu
        /// </summary>
        /// <param name="classId">班级ID</param>
        public void DeleteClassByClassId(long classId)
        {
            if (classId < 0)
            {
                //classId不合法时抛出异常
                throw new ArgumentException("classId格式错误");
            }

            ClassInfo classinfo = _db.ClassInfo.Find(classId);
            if (classinfo == null)
                throw new ClassNotFoundException();
            /*
            //级联查找FixGroup实体集
            List<FixGroup> fgList = _db.FixGroup.Include(c => c.ClassInfo).ToList();

            //根据classinfo找到fixgroup实体
            List<FixGroup> fgListToDelete = fgList.FindAll(c => c.ClassInfo==classinfo);

            //级联查找courseSelection表中的记录
            List<CourseSelection> csList = _db.CourseSelection.Include(c => c.ClassInfo).ToList();

            //根据classinfo找到courseselection实体
            List<CourseSelection> csListToDelete = csList.FindAll(c => csListToDelete.Contains(c.ClassInfo));
            */
            _db.ClassInfo.RemoveRange(_db.ClassInfo.Where(c => c.Id == classId));
            _db.CourseSelection.Remove(_db.CourseSelection.SingleOrDefault(c =>c.ClassInfo.Id == classId));
            _db.FixGroup.RemoveRange(_db.FixGroup.Where(f => f.ClassInfo.Id == classId));
            _db.SaveChanges();
        }

        /// <summary>
        /// 按courseId删除Class.
        /// @author cuiheyu
        /// 先根据CourseId获得所有的class的信息，然后根据class信息删除courseSelection表的记录，然后再根据courseId和classId删除ScoureRule表记录，再调用根据classId删除固定分组，最后再将班级的信息删除<br>
        /// </summary>
        /// @param courseId 课程Id
        /// @throws CourseNotFoundException 无此Id的课程
        /// @author zhouzhongjun
        /// @see ClassService #listClassByCourseId(BigInteger courseId)
        /// @see ClassService #deleteClasssSelectionByClassId(BigInteger classId)
        /// @see FixGroupService #deleteFixGroupByClassId(BigInteger ClassId
        /// <param name="courseId">课程Id</param>
        /// 
        
        public void DeleteClassByCourseId(long courseId)
        {
            if (courseId < 0)
            {
                throw new ArgumentException("courseId不合法");
            }

            //根据courseId找到course实体
            Course course = _db.Course.Find(courseId);
            //找不到course时抛出
            if (course == null)
                throw new CourseNotFoundException();

            //级联查找classinfo实体集
            List<ClassInfo> ciList = _db.ClassInfo.Include(c => c.Course).ToList();

            //根据course实体找到classInfo实体集
            List<ClassInfo> ciListToDelete = ciList.FindAll(c => c.Course == course);

            //级联查找courseSelection表中的记录
            List<CourseSelection> csList = _db.CourseSelection.Include(c => c.ClassInfo).ToList();

            //根据classinfo找到courseselection实体
            List<CourseSelection> csListToDelete = csList.FindAll(c => ciListToDelete.Contains(c.ClassInfo));

            //级联查找FixGroup实体集
            List<FixGroup> fgList = _db.FixGroup.Include(c => c.ClassInfo).ToList();

            //根据classinfo找到fixgroup实体
            List<FixGroup> fgListToDelete = fgList.FindAll(c => ciListToDelete.Contains(c.ClassInfo));

            _db.ClassInfo.RemoveRange(ciListToDelete);
            _db.CourseSelection.RemoveRange(csListToDelete);
            _db.FixGroup.RemoveRange(fgListToDelete);

            //提交事务
            _db.SaveChanges();
        }
        

        /// <summary>
        /// 按classId删除CourseSelection表的一条记录.
        /// @author cuiheyu
        /// </summary>
        /// <param name="classId">班级Id</param>
        public void DeleteClassSelectionByClassId(long classId)
        {
            if (classId < 0)
            {
                throw new ArgumentException();
            }
            _db.CourseSelection.Remove(_db.CourseSelection.SingleOrDefault(c => c.ClassInfo.Id == classId));
            _db.SaveChanges();
        }

        /// <summary>
        /// 根据班级id和用户id删除选课记录及与班级相关的信息
        /// 学生按班级id取消选择课程.
        /// 按StudentId删除CourseSelection表
        /// @author cuiheyu
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="classId">班级id</param>
        ///   @throws UserNotFoundException  无此姓名的教师
        ///   @throws ClassesNotFoundException 无此Id的班级
        public void DeleteCourseSelectionById(long userId, long classId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("userId格式错误");
            }
            if (classId <= 0)
            {
                throw new ArgumentException("classId格式错误");
            }

            UserInfo user = _db.UserInfo.Find(userId);
            if (user == null) throw new UserNotFoundException();

            ClassInfo classinfo = _db.ClassInfo.Find(classId);
            if (classinfo == null) throw new ClassNotFoundException();

            //连接三表查询
            //List<CourseSelection> csList = _db.CourseSelection.Include(c => c.UserInfo).Include(c => c.ClassInfo).Where(c => c.ClassInfo == classinfo && c.UserId = user).ToList();

            _db.CourseSelection.RemoveRange(_db.CourseSelection.Include(c => (c.Student.Id == userId) && (c.ClassInfo.Id == classId)));
            _db.SaveChanges();
        }

        /// <summary>
        /// 学生按班级id选择班级.
        /// 根据班级id和用户id新增选课记录
        /// @author cuiheyu
        /// </summary>
        public long InsertCourseSelectionById(long userId, long classId)
        {
            if (userId <= 0)
                throw new ArgumentException("userId错误");
            if (classId <= 0)
                throw new ArgumentException("classId错误");

            var student = _db.UserInfo.SingleOrDefault(s => s.Id == userId);
            if (student == null)
                throw new UserNotFoundException();

            var classinfo = _db.ClassInfo.SingleOrDefault(c => c.Id == classId);
            if (classinfo == null)
                throw new ClassNotFoundException();

            var isExist = _db.CourseSelection.Include(cs => cs.ClassInfo).Include(cs => cs.Student).Where(cs => cs.ClassInfo.Id == classId && cs.Student.Id == userId);

            if (isExist.Any())
            {
                throw new InvalidOperationException();
            }

            var classinformation = _db.CourseSelection.Add(new CourseSelection
            {
                ClassInfo = classinfo,
                Student = student
            });
            _db.SaveChanges();

            return classinformation.Entity.Id;
        }
        /// <summary>
        /// 老师发起签到.
        /// @author cuiheyu
        /// </summary>
        /// <p>往location表插入一条当前讨论课班级的签到状态<br> 
        /// @param location 当前讨论课班级的签到状态记录
        /// @return 返回location表的新记录的id
        /// @throws SeminarNotFoundException 讨论课没有找到
        /// @throws ClassNotFoundException   无此Id的班级
        public long CallInRollById(Location location)
        {
            if (location.ClassInfo == null)
                throw new ClassNotFoundException();
            if (location.Seminar == null)
                throw new SeminarNotFoundException();

            var callinroll = _db.Location.Add(location);
            _db.SaveChanges();
            return callinroll.Entity.Id;
        }


        /// <summary>
        /// 老师获取位置信息，获取班级签到状态.
        /// @author dhd
        /// </summary>
        /// <param name="seminarId">讨论课id</param>
        /// <param name="classId">班级id</param>
        /// <returns>location 班级签到状态</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupBySeminarId(System.Int64)"/>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.SeminarNotFoundException">无此ID的讨论课</exception>
        public Location GetCallStatusById(long seminarId, long classId)
        {
            ///var seminar = _db.Location.Where(s => (s.Seminar.Id == seminarId));
            if ( _db.Seminar.Find(seminarId) == null)
                throw new SeminarNotFoundException();
            var location = _db.Location
                .Include(l => l.Seminar)
                .Include(l=>l.ClassInfo)
                .SingleOrDefault(s => (s.Seminar.Id == seminarId && s.ClassInfo.Id == classId));
            return location;

        }

        /// <summary>
        /// 按班级id获取班级详情.
        /// @author dhd
        /// </summary>
        /// <param name="classId">班级ID</param>
        /// <returns>ClassBO 班级</returns>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ClassNotFoundExceptin">无此ID的班级</exception>
        public ClassInfo GetClassByClassId(long classId)
        {
            var classInfo = _db.ClassInfo.SingleOrDefault(c => c.Id == classId);
            if (classInfo == null)
                throw new ClassNotFoundException();
            return classInfo;
        }

        /*
         * 已删除
        /// <summary>
        /// 查询评分规则.
        /// @author dhd
        /// </summary>
        /// <param name="classId">班级id</param>
        /// <returns>ProportionBO 返回评分规则，若未找到对应评分规则返回空（null)</returns>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ClassNotFoundExceptin">无此ID的班级</exception>
        public ClassInfo GetScoreRule(long classId)
        {
            var classInfo = _db.ClassInfo.SingleOrDefault(c => c.Id == classId);
            if (classInfo == null)
                throw new ClassNotFoundException();
            return classInfo;
        }
        */

        /// <summary>
        /// 根据课程ID获得班级列表.
        /// @author dhd
        /// </summary>
        /// <param name="courseId">课程ID</param>
        /// <returns>list 班级列表</returns>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.CourseNotFoundExceptin">无此名称的课程</exception>
        public IList<ClassInfo> ListClassByCourseId(long courseId)
        {
            if (_db.Course.Find(courseId) == null)
                throw new CourseNotFoundException();
            var classes = _db.ClassInfo
                .Include(c => c.Course)
                .Where(c => c.Course.Id == courseId)
                .ToList();
            return classes;
        }
        /*
         * 已删除
        /// <summary>
        /// 按课程名称和教师名称获取班级列表.
        /// @author dhd
        /// </summary>
        /// <param name="courseName">课程名称</param>
        /// <param name="teacherName">教师名称</param>
        /// <returns>List 班级列表</returns>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.UserNotFoundExceptin">无此姓名的教师</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.CourseNotFoundExceptin">无此名称的课程</exception>
        public IList<ClassInfo> ListClassByName(string courseName, string teacherName)
        {
            var teacher = _db.UserInfo.FirstOrDefault(u => (u.Name == teacherName &&u.Type == Shared.Models.Type.Teacher));
            if(teacher==null)
                throw new UserNotFoundException();

            var course = _db.Course.Include(c=>c.Teacher==teacher)
                .FirstOrDefault(c => c.Name==courseName);
            if(course==null)
                throw new CourseNotFoundException();
            var classes = _db.ClassInfo.Where(c=>c.Course==course).ToList();
            return classes;
        }
        */

        /// <summary>
        /// 按班级id和班级修改班级信息.
        /// @author dhd
        /// </summary>
        /// <param name="classId">班级ID</param>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ClassNotFoundExceptin">无此ID的班级</exception>
        public void UpdateClassByClassId(long classId, ClassInfo newclass)
        {
            var classInfo = _db.ClassInfo.SingleOrDefault(c => c.Id == classId);
            if (classInfo == null)
                throw new ClassNotFoundException();
            classInfo.Name = newclass.Name;
            classInfo.Site = newclass.Site;
            classInfo.ClassTime = classInfo.ClassTime;
            classInfo.ReportPercentage = newclass.ReportPercentage;
            classInfo.PresentationPercentage = newclass.PresentationPercentage;
            classInfo.FivePointPercentage = newclass.FivePointPercentage;
            classInfo.FourPointPercentage = newclass.FourPointPercentage;
            classInfo.ThreePointPercentage = newclass.ThreePointPercentage;
            _db.SaveChanges();
        }

        /*
         * 已删除
        /// <summary>
        /// 修改评分规则.
        /// @author dhd
        /// </summary>
        /// <param name="classId">班级id</param>
        /// <param name="proportions">评分规则</param>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ArgumentException">班级信息不合法</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ClassNotFoundExceptin">无此ID的班级</exception>
        public void UpdateScoreRule(long classId, ClassInfo proportions)
        {
            if (_db.ClassInfo.Find(classId) == null)
                throw new ClassNotFoundException();
            if (proportions.ReportPercentage <= 0 || proportions.PresentationPercentage <= 0 ||
                proportions.FivePointPercentage <= 0 || proportions.FourPointPercentage <= 0 |
                proportions.ThreePointPercentage <= 0)
                throw new ArgumentException("班级信息不合法");
            var classInfo = _db.ClassInfo.SingleOrDefault(c => c.Id == classId);
            classInfo.ReportPercentage = proportions.ReportPercentage;
            classInfo.PresentationPercentage = proportions.PresentationPercentage;
            classInfo.FivePointPercentage = proportions.FivePointPercentage;
            classInfo.FourPointPercentage = proportions.FourPointPercentage;
            classInfo.ThreePointPercentage = proportions.ThreePointPercentage;
            _db.SaveChanges();
        }
        */

        /// <summary>
        /// 根据学生ID获取班级列表
        /// @author dhd
        /// </summary>
        /// <param name="userId">班级id</param>
        /// <returns>list 班级列表</returns>
        /// <exception cref="T:System.ArgumentException">userID格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.UserNotFoundExceptin">无此ID的用户</exception>
        public List<ClassInfo> ListClassByUserId(long userId)
        {
            if (userId < 0)
                throw new ArgumentException("UserID格式错误");
            if (_db.UserInfo.Find(userId) == null)
                throw new UserNotFoundException();
            var classes = (from cs in _db.CourseSelection
                           where cs.Student.Id == userId
                           select cs.ClassInfo).ToList();
            return classes;
        }


        /// <summary>
        /// 新增老师结束签到
        /// @author dhd
        /// 老师结束签到,修改当前讨论课班级的签到状态为已结束
        /// </summary>
        /// <param name="location">当前讨论课班级的签到状态记录</param>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.SeminarNotFoundException">讨论课没有找到</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ClassesNotFoundException">无此Id的班级</exception>
        public void EndCallRollById(long seminarId, long classId)
        {
            if (_db.Seminar.Find(seminarId) == null)
                throw new SeminarNotFoundException();
            if (_db.ClassInfo.Find(classId) == null)
                throw new ClassNotFoundException();
            var location = GetCallStatusById(seminarId, classId);
            location.Status = 0;
            _db.SaveChanges();
        }
    }
}

