﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoteSystem.Data.Entities.PollAggregate;
using VoteSystem.Data.Repositories;

namespace VoteSystem.EF.Repositories
{
    public class PollRepository : IPollRepository
    {
        public void Create(Poll poll)
        {
            using (var ctx = new VoteContext())
            {
                ctx.Polls.Add(poll);
                ctx.SaveChanges();
            }
        }
        public void CreateChoice(Choice choice, int pollId)
        {
            using (var ctx = new VoteContext())
            {
                if(ctx.Polls.FirstOrDefault(p => p.Id == pollId).Choices == null);
                    ctx.Polls.FirstOrDefault(p => p.Id == pollId).Choices = new List<Choice>();
                ctx.Choices.Add(choice);
                ctx.Polls.FirstOrDefault(p => p.Id == pollId).Choices.Add(choice);
                ctx.SaveChanges();
            }
        }
        public void AddChoiceToPoll(Choice choice, int pollId)
        {
            using (var ctx = new VoteContext())
            {
                ctx.Polls.FirstOrDefault(p => p.Id == pollId).Choices.Add(choice);
                ctx.SaveChanges();
            }
        }
        public Poll Get(int id)
        {
            using (var ctx = new VoteContext())
                return ctx.Polls.FirstOrDefault(p => p.Id == id);
        }
        public Poll Get(string pollName)
        {
            using (var ctx = new VoteContext())
            {
                return ctx.Polls.FirstOrDefault(p => p.Name == pollName);
            }
        }
        public List<Choice> GetChoices(int pollId)
        {
            using (var ctx = new VoteContext())
            {
                List<Choice> choices = ctx.Polls.FirstOrDefault(p => p.Id == pollId).Choices;
                if (choices == null)
                    choices = new List<Choice>();
                return choices;
            }
        }

        public Choice GetChoice(int choiceId)
        {
            using (var ctx = new VoteContext())
                return ctx.Choices.FirstOrDefault(c => c.Id == choiceId);
        }
        public string[] GetPollDetails(int id)
        {
            using (var ctx = new VoteContext())
            {
                Poll poll = ctx.Polls.FirstOrDefault(p => p.Id == id);
                string[] returnstring;
                if (poll != null)
                {
                    returnstring = new string[2];
                    returnstring[0] = poll.Name;
                    returnstring[1] = poll.Description;
                }
                else
                {
                    returnstring = new string[1];
                    returnstring[0] = "Sorry, there is not any appropriate poll";
                }
                return returnstring;
            }
        }

        public List<Poll> GetPolls(int id)
        {
            using (var ctx = new VoteContext())
            {
                return ctx.Polls.ToList();
            }
        }
        public void Update(Poll poll)
        {
            using (var ctx = new VoteContext())
            {
                ctx.Polls.FirstOrDefault(p => p.Id == poll.Id).Name = poll.Name;
                ctx.Polls.FirstOrDefault(p => p.Id == poll.Id).Description = poll.Description;
                ctx.Polls.FirstOrDefault(p => p.Id == poll.Id).Choices = poll.Choices;
                ctx.SaveChangesAsync();
            }
        }
    }
}
