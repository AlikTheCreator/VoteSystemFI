using System;
using System.Collections.Generic;
using System.Text;
using VoteSystem.Data.Entities.PollAggregate;

namespace VoteSystem.Data.Repositories
{
    public interface IPollRepository
    {
        int Create(Poll poll);
        void CreateChoice(Choice choice, int pollId);
        List<Choice> GetChoices(int pollId);
        void AddChoiceToPoll(Choice choice, int pollId);
        void Update(Poll poll);
        Poll Get(int id);
        Poll Get(string pollName);
        int? GetPollId(string pollName);
        List<Poll> GetPolls();
        string[] GetPollDetails(int id);
        Choice GetChoice(int choiceId);
    }
}
