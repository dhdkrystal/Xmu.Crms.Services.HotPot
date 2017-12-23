using System;
using System.Collections.Generic;
using System.Linq;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;


namespace Xmu.Crms.Services.HotPot
{
    public class ClassService : IClassService
    {
        private readonly CrmsContext _db;

        /// <summary>
        /// 按班级id删除班级.
        /// @author yexiaona
        /// </summary>
        /// <param name="classId">班级ID</param>
        public void DeleteClassByClassId(long classId)
        {
            _db.RemoveRange(_db.ClassInfo.Where(c => c.Id == classId));
            _db.SaveChanges();
        }

        /// <summary>
        /// 按courseId删除Class.
        /// @author zhouzhongjun
        /// </summary>
        /// <param name="courseId">课程Id</param>
        public void DeleteClassByCourseId(long courseId)
        {
            _db.RemoveRange(_db.ClassInfo.Where(c => c.Course.Id == courseId));
            _db.SaveChanges();
        }
        /// <summary>
        /// 按classId删除CourseSelection表的一条记录.
        /// @author zhouzhongjun
        /// </summary>
        /// <param name="classId">班级Id</param>
        public void DeleteClassSelectionByClassId(long classId)
        {
            _db.Remove(_db.CourseSelection.Where(c => c.ClassInfo.Id == classId));
            _db.SaveChanges();
        }

        /// <summary>
        /// 学生按班级id取消选择班级.
        /// @author yexiaona
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="classId">班级id</param>
        public void DeleteCourseSelectionById(long userId, long classId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按classId删除ScoreRule.
        /// @author zhouzhongjun
        /// </summary>
        /// <param name="classId">班级Id</param>
        public void DeleteScoreRuleById(long classId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 老师获取位置信息，获取班级签到状态.
        /// 
        /// @author yexiaona
        /// </summary>
        /// <param name="seminarId">讨论课id</param>
        /// <param name="classId">班级id</param>
        /// <returns>location 班级签到状态</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupBySeminarId(System.Int64)"/>
        public Location GetCallStatusById(long seminarId, long classId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按班级id获取班级详情.
        /// @author yexiaona
        /// </summary>
        /// <param name="classId">班级ID</param>
        /// <returns>ClassBO 班级</returns>
        public ClassInfo GetClassByClassId(long classId)
        {
            var classInfo = _db.ClassInfo.SingleOrDefault(c => c.Id == classId);
            return classInfo;
        }

        /// <summary>
        /// 查询评分规则.
        /// @author YeHongjie
        /// </summary>
        /// <param name="classId">班级id</param>
        /// <returns>ProportionBO 返回评分规则，若未找到对应评分规则返回空（null)</returns>
        public ClassInfo GetScoreRule(long classId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 新建班级.
        /// @author yexiaona
        /// </summary>
        /// <param name="userId">教师id</param>
        /// <param name="courseId">课程id</param>
        /// <param name="classInfo">班级信息</param>
        /// <returns>classId 班级Id</returns>
        public long InsertClassById(long userId, long courseId, ClassInfo classInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 学生按班级id选择班级.
        /// @author yexiaona
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="classId">班级id</param>
        /// <returns>url 选课url</returns>
        public string InsertCourseSelectionById(long userId, long classId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 新增评分规则.
        /// @author YeHongjie
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="proportions">评分规则</param>
        /// <returns>scoreRuleId 若创建成功则返回该评分规则的id，失败则返回-1</returns>
        public long InsertScoreRule(long classId, ClassInfo proportions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据课程ID获得班级列表.
        /// @author yexiaona
        /// </summary>
        /// <param name="courseId">课程ID</param>
        /// <returns>list 班级列表</returns>
        public IList<ClassInfo> ListClassByCourseId(long courseId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按课程名称和教师名称获取班级列表.
        /// @author yexiaona
        /// </summary>
        /// <param name="courseName">课程名称</param>
        /// <param name="teacherName">教师名称</param>
        /// <returns>List 班级列表</returns>
        public IList<ClassInfo> ListClassByName(string courseName, string teacherName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按班级id和班级修改班级信息.
        /// @author yexiaona
        /// </summary>
        /// <param name="classId">班级ID</param>
        public void UpdateClassByClassId(long classId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 修改评分规则.
        /// @author YeHongjie
        /// </summary>
        /// <param name="classId">班级id</param>
        /// <param name="proportions">评分规则</param>
        public void UpdateScoreRule(long classId, ClassInfo proportions)
        {
            throw new NotImplementedException();
        }
    }
}
