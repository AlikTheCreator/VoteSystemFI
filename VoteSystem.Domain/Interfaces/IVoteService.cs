using System;
using System.Collections.Generic;
using System.Text;
using VoteSystem.Data.Entities.UserPolicyAggregate;
using VoteSystem.Data.Entities.VoteAggregate;

namespace VoteSystem.Domain.Interfaces
{
    public interface IVoteService
    {
        public Vote Vote(int userId, int Idchoice);
        public bool CheckIfVoted(int userId, string pollName);
    }
}
