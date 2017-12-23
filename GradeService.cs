using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System.Linq;


namespace Xmu.Crms.Services.HotPot
{
    public class GradeService : IGradeService
    {
        private readonly CrmsContext _db;
        public GradeService(CrmsContext db)
        {
            _db = db;
        }

        public void CountGroupGradeBySerminarId(long seminarId, long seminarGroupId)
        {
            throw new NotImplementedException();
        }

        public void CountPresentationGrade(long seminarId, long seminarGroupId)
        {
            throw new NotImplementedException();
        }

        public void DeleteStudentScoreGroupByTopicId(long topicId)
        {
            throw new NotImplementedException();
        }

        public SeminarGroup GetSeminarGroupBySeminarGroupId(long userId, long seminarGroupId)
        {
            throw new NotImplementedException();
        }

        public void InsertGroupGradeByUserId(long topicId, long userId, long seminarId, long groupId, int grade)
        {
            throw new NotImplementedException();
        }

        public IList<SeminarGroup> ListSeminarGradeByCourseId(long userId, long courseId)
        {
            throw new NotImplementedException();
        }

        public IList<SeminarGroup> ListSeminarGradeByStudentId(long userId)
        {
            throw new NotImplementedException();
        }

        public void UpdateGroupByGroupId(long seminarGroupId, int grade)
        {
            throw new NotImplementedException();
        }
    }
}
