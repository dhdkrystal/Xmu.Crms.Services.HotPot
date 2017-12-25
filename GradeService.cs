using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Xmu.Crms.Services.HotPot
{
    public class GradeService : IGradeService
    {
        private readonly CrmsContext _db;
        private readonly ISeminarGroupService _iSeminarGroupService;
        private readonly ISeminarService _iSeminarService;
        public GradeService(CrmsContext db, ISeminarGroupService iSeminarGroupService,ISeminarService iSeminarService)
        {
            _db = db;
            _iSeminarGroupService = iSeminarGroupService;
            _iSeminarService = iSeminarService;
        }

        //快排先放这里。实现类里出现了接口里未有的函数，更新后应该会报错。视情况考虑写成一个新文件然后引用进来
        public void QuickSort(long[] idList, double[] gradeList, int low, int high)
        {
            if (low >= high) return;
            Random ran = new Random();
            int flag = ran.Next(low, high);
            long id = idList[flag];
            double grade = gradeList[flag];
            int i = low;
            int j = high;
            while (i < j)
            {
                while (gradeList[j] <= grade && i < j) j--;
                if (i < j)
                {
                    Change(idList, gradeList, i, j);
                    i++;
                }
                while (gradeList[j] >= grade && i < j) i++;
                if (i < j)
                {
                    Change(idList, gradeList, i, j);
                    j--;
                }
            }
            gradeList[i] = grade;
            idList[i] = id;
            QuickSort(idList, gradeList, low, i - 1);
            QuickSort(idList, gradeList, i + 1, high);
        }
        public void Change(long[] idList, double[] gradeList, int i, int j)
        {
            long id = idList[i];
            double grade = gradeList[i];
            idList[i] = idList[j];
            gradeList[i] = gradeList[j];
            idList[j] = id;
            gradeList[j] = grade;
        }
        /**
        * 按seminarGroupTopicId删除学生打分表.
        *
         * @param seminarGroupTopicId  小组话题表的Id
         * @throws IllegalArgumentException topicId格式错误时抛出
         * @author fengjinliu
         */
        public void DeleteStudentScoreGroupByTopicId(long seminartopicId)
        {
            /*
            //查找到所有的seminarGroupTopic
            //查找到所有的studentScoreGroup//.AsNoTracking()
            List<SeminarGroupTopic> seminarGroupTopicList = _db.SeminarGroupTopic.Where(u => u.seminartopic.Id == seminartopicId).ToList();
            if (seminarGroupTopicList == null)
                throw new GroupNotFoundException();
            foreach (var seminarGroupTopic in seminarGroupTopicList)
            {
                List<StudentScoreGroup> studentScoreGroupList = _db.StudentScoreGroup.Include(u => u.SeminarGroupTopic).Where(u => u.SeminarGroupTopic.Id == seminarGroupTopic.Id).ToList();
                foreach (var studentScoreGroup in studentScoreGroupList)
                {
                    //将实体附加到对象管理器中
                    _db.StudentScoreGroup.Attach(studentScoreGroup);
                    //删除
                    _db.StudentScoreGroup.Remove(studentScoreGroup);
                }
            }
            _db.SaveChanges();
            */

        }
        /// <summary>
        /// 仅作为普通方法，被下面的定时器方法调用.
        /// 讨论课结束后计算展示得分.
        /// @author fengjinliu
        /// 条件: 讨论课已结束  *GradeService
        /// </summary>
        /// <param name="seminarId">讨论课id</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        public void CountPresentationGrade(long seminarId, IList<Topic> topicList)
        {
            foreach (var topic in topicList)
            {
                //通过seminarGroupId获得List<SeminarGroupTopic>  
                //通过seminarGrouptopicId获得List<StudentScoreGroup>
                long[] idList;
                double[] gradeList;

                //获取选择该topic的所有小组
                var seminarGroupTopicList = _db.SeminarGroupTopic.Include(u => u.SeminarGroup).Include(u => u.Topic).Where(u => u.Topic.Id == topic.Id).ToList();
                if (seminarGroupTopicList == null) throw new GroupNotFoundException();
                else
                {
                    idList = new long[seminarGroupTopicList.Count];
                    gradeList = new double[seminarGroupTopicList.Count];

                    int groupNumber = 0;

                    //计算一个小组的所有学生打分情况
                    foreach (var i in seminarGroupTopicList)
                    {
                        //List<StudentScoreGroup> studentScoreList = new List<StudentScoreGroup>();
                        //获取学生打分列表
                        var studentScoreList = _db.StudentScoreGroup.Where(u => u.SeminarGroupTopic.Id == i.Id).ToList();
                        if (studentScoreList == null)//该组没有被打分
                            seminarGroupTopicList.Remove(i);
                        int? grade = 0; int k = 0;
                        foreach (var g in studentScoreList)
                        {
                            grade += g.Grade;
                            k++;
                        }
                        double avg = (double)grade / k;

                        //将小组该讨论课平均分和Id保存
                        idList[groupNumber] = i.Id;
                        gradeList[groupNumber] = avg;
                        groupNumber++;
                    }
                    //将小组成绩从大到小排序
                    QuickSort(idList, gradeList, 0, groupNumber - 1);

                    Seminar seminar;
                    ClassInfo classInfo;
                    try
                    {
                        seminar = _db.Seminar.Include(u => u.Course).Where(u => u.Id == seminarId).SingleOrDefault();
                        if (seminar == null) throw new SeminarNotFoundException();
                        classInfo = _db.ClassInfo.Where(u => u.Id == seminar.Course.Id).SingleOrDefault();
                        if (classInfo == null) throw new ClassNotFoundException();
                    }
                    catch
                    {
                        throw;
                    }
                    //各小组按比例给分
                    int Five = Convert.ToInt32(groupNumber * classInfo.FivePointPercentage * 0.01);
                    int Four = Convert.ToInt32(groupNumber * classInfo.FourPointPercentage * 0.01);
                    int Three = Convert.ToInt32(groupNumber * classInfo.ThreePointPercentage * 0.01);
                    for (int i = 0; i < groupNumber; i++)
                    {
                        SeminarGroupTopic seminarGroupTopic = _db.SeminarGroupTopic.SingleOrDefault(s => s.Id == idList[i]);
                        //如果找不到该组
                        if (seminarGroupTopic == null)
                        {
                            throw new GroupNotFoundException();
                        }
                        //更新报告分
                        if (i >= 0 && i < Five) seminarGroupTopic.PresentationGrade = 5;
                        if (i >= Five && i < Four) seminarGroupTopic.PresentationGrade = 4;
                        if (i >= Four && i < groupNumber) seminarGroupTopic.PresentationGrade = 3;
                        _db.SaveChanges();
                    }

                }//if end

            }//foreach topic end
        }



        /// <summary>
        /// 定时器方法:讨论课结束后计算本次讨论课得分.
        /// @author fengjinliu
        /// 条件: 讨论课已结束，展示得分已算出  *GradeService
        /// </summary>
        /// <param name="seminarId">讨论课id</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// 

        public void CountGroupGradeBySerminarId(long seminarId, IList<SeminarGroup> seminarGroupList)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                //根据seminarGroupList中元素，依次计算
                //SeminarGroup实体中保存了ClassInfo实体对象，可以查到成绩计算方法
                double[] tempTotalGrade = new double[seminarGroupList.Count];
                long[] tempId = new long[seminarGroupList.Count];
                foreach (var seminarGroup in seminarGroupList)
                {
                    //根据seminarGroupId获得seminarGroupTopicList,计算每一个seminarGroup的展示分数
                    List<SeminarGroupTopic> seminarGroupTopicList = _db.SeminarGroupTopic.Include(u => u.SeminarGroup).Where(u => u.SeminarGroup.Id == seminarGroup.Id).ToList();
                    if (seminarGroupTopicList == null)//该组没有展示分数
                        seminarGroupList.Remove(seminarGroup);
                    int? grade = 0;
                    int number = 0;
                    foreach (var seminarGroupTopic in seminarGroupTopicList)
                    {
                        grade += seminarGroupTopic.PresentationGrade;
                        number++;
                    }
                    try
                    {
                        //更新seminarGroup中的展示成绩
                        int? avgPreGrade = grade / number;
                        _db.SeminarGroup.Attach(seminarGroup);
                        seminarGroup.PresentationGrade = avgPreGrade;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback(); throw;
                    }

                }
                for (int i = 0; i < seminarGroupList.Count; i++)
                {
                    tempTotalGrade[i] = ((double)(seminarGroupList[i].ClassInfo.PresentationPercentage * seminarGroupList[i].PresentationGrade
                        + seminarGroupList[i].ClassInfo.ReportPercentage * seminarGroupList[i].ReportGrade)) / 100;
                    tempId[i] = seminarGroupList[i].Id;
                }
                //排序
                //将小组总成绩从大到小排序
                QuickSort(tempId, tempTotalGrade, 0, seminarGroupList.Count - 1);
                //根据排序和比例计算组数
                int A = Convert.ToInt32(seminarGroupList.Count * seminarGroupList[0].ClassInfo.FivePointPercentage * 0.01);
                int B = Convert.ToInt32(seminarGroupList.Count * seminarGroupList[0].ClassInfo.FourPointPercentage * 0.01);
                int C = Convert.ToInt32(seminarGroupList.Count * seminarGroupList[0].ClassInfo.ThreePointPercentage * 0.01);

                //各小组按比例给分
                for (int i = 0; i < A; i++)
                {
                    try
                    {
                        ////更新报告分
                        //_db.SeminarGroup.Attach(seminarGroupList[i]);
                        //seminarGroupList[i].FinalGrade = 3;
                        //_db.SaveChanges();

                        SeminarGroup seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == tempId[i]);
                        //如果找不到该组
                        if (seminarGroup == null)
                        {
                            throw new GroupNotFoundException();
                        }
                        //更新报告分
                        seminarGroup.FinalGrade = 5;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }
                for (int i = B; i < B + C; i++)
                {
                    try
                    {
                        SeminarGroup seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == tempId[i]);
                        //如果找不到该组
                        if (seminarGroup == null)
                        {
                            throw new GroupNotFoundException();
                        }
                        //更新报告分
                        seminarGroup.FinalGrade = 4;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }
                for (int i = B + C; i < seminarGroupList.Count; i++)
                {
                    try
                    {
                        SeminarGroup seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == tempId[i]);
                        //如果找不到该组
                        if (seminarGroup == null)
                        {
                            throw new GroupNotFoundException();
                        }
                        //更新报告分
                        seminarGroup.FinalGrade = 3;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }
            }//using scope end
        }



        //@author cuisy

        //<summary>按讨论课小组id获取讨论课小组</summary>
        //<param name="userId">讨论课小组Id</param>
        //<param name="seminarGroupId">讨论课小组Id</param>
        //<returns>SeminarGroup 讨论课小组信息</returns>
        //<exception cref="T:System.ArgumentException">SeminarId格式错误</exception>
        //<exception cref="T:System.ArgumentException">UserId格式错误</exception>
        public SeminarGroup GetSeminarGroupBySeminarGroupId(long seminarGroupId)
        {
            if (seminarGroupId <= 0)
                throw new ArgumentException(nameof(seminarGroupId));

            SeminarGroup group = _db.SeminarGroup.SingleOrDefault(c => c.Id == seminarGroupId)
                ?? throw new InvalidOperationException();

            return group;

        }

        //<summary>提交对其他小组的打分</summary>
        //<param name="topicId">话题Id</param>
        //<param name="userId">用户Id</param>
        //<param name="seminarId">讨论课Id</param>
        //<param name="groupId">小组Id</param>
        //<param name="grade">成绩</param>
        //<returns></returns>
        public void InsertGroupGradeByUserId(long topicId, long userId, long seminarId, long groupId, int grade)
        {
            if (topicId <= 0) throw new ArgumentException(nameof(topicId));
            if (userId <= 0) throw new ArgumentException(nameof(userId));
            if (seminarId <= 0) throw new ArgumentException(nameof(seminarId));
            if (groupId <= 0) throw new ArgumentException(nameof(groupId));

            var usr = _db.UserInfo.Find(userId) ?? throw new UserNotFoundException();
            var tpc = _db.Topic.Find(topicId) ?? throw new TopicNotFoundException();
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            var smg = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();

            var sgt = _db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .Where(s => s.Topic == tpc).SingleOrDefault(s => s.SeminarGroup == smg)
                ?? throw new InvalidOperationException();

            var ssg = _db.StudentScoreGroup.Include(m => m.SeminarGroupTopic).Include(m => m.Student)
                .Where(m => m.Student == usr).SingleOrDefault(m => m.SeminarGroupTopic == sgt)
                ?? throw new InvalidOperationException();

            ssg.Grade = grade;
            _db.SaveChanges();
        }

        /// <summary>
        /// 按课程id获取学生该课程所有讨论课
        /// 通过课程id获取该课程下学生所有讨论课详细信息（包括成绩）
        /// </summary>
        /// <param name="userId">学生id</param>
        /// <param name="courseId">课程id</param>
        /// <returns>list 该课程下所有讨论课列表</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarService.ListSeminarByCourseId(System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IGradeService.ListSeminarGradeByUserId(System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupBySeminarId(System.Int64)"/>
        public IList<SeminarGroup> ListSeminarGradeByCourseId(long userId, long courseId)
        {
            if (userId <= 0) throw new ArgumentException(nameof(userId));
            if (courseId <= 0) throw new ArgumentException(nameof(courseId));

            /*var usr = _db.UserInfo.Find(userId) ?? throw new UserNotFoundException();
            var cor = _db.Course.Find(courseId) ?? throw new CourseNotFoundException();
            var sem = _db.Seminar.Find(cor) ?? throw new SeminarNotFoundException();
            var sgm = _db.SeminarGroupMember.Include(s => s.Student).ThenInclude(sem => sem.Seminar)
                .Where(s => s.Student == usr && sem.Seminar == sem)
                ?? throw new InvalidOperationException();

            return sgm.SeminarGroup.ToList();*/

            List<SeminarGroup> seminarGroupList = new List<SeminarGroup>();
            List<Seminar> seminarList = new List<Seminar>();

            //调用SeminarService 中 IList<Seminar> ListSeminarByCourseId(long courseId)方法
            _iSeminarService.ListSeminarByCourseId(courseId);
            //调用SeminarGroupService 中 SeminarGroup GetSeminarGroupById(long seminarId, long userId)
            for (int i = 0; i < seminarList.Count; i++)
                seminarGroupList.Add(_iSeminarGroupService.GetSeminarGroupById(seminarList[0].Id, userId));

            return seminarGroupList;
        }

        /// <summary>
        /// 按ID设置小组报告分.
        /// </summary>
        /// <param name="seminarGroupId">讨论课组id</param>
        /// <param name="grade">分数</param>
        public void UpdateGroupByGroupId(long seminarGroupId, int grade)
        {
            if (seminarGroupId <= 0) throw new ArgumentException(nameof(seminarGroupId));

            SeminarGroup smg = _db.SeminarGroup.Find(seminarGroupId);
            smg.ReportGrade = grade;

            _db.SaveChanges();
        }


        public void InsertGroupGradeByUserId(long topicId, long userId, long groupId, int grade)
        {
            throw new NotImplementedException();
        }

        public void CountPresentationGrade(long seminarId)
        {
            throw new NotImplementedException();
        }

        public void CountGroupGradeBySerminarId(long seminarId)
        {
            throw new NotImplementedException();
        }
    }
}