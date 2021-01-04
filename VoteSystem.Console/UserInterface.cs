using System;
using System.Collections.Generic;
using System.Text;
using VoteSystem.Data.DTO;
using VoteSystem.Data.Entities.PollAggregate;
using VoteSystem.Data.Repositories;
using VoteSystem.Domain.Interfaces;
using VoteSystem.EF.Repositories;

namespace VoteSystem.Cosnole
{
    public class UserInterface
    {
        IUserRepository _userRepository;
        IAuthorizationContext _authorizationContext;
        IPollRepository _pollRepos;
        IPolicyChecker _policyChecker;
        IPollService _pollService;
        public UserInterface(IUserRepository userRepository, IAuthorizationContext authorizationContext, 
                             IPollRepository pollRepository, IPolicyChecker policyChecker,
                             IPollService pollService)
        {
            _userRepository = userRepository;
            _authorizationContext = authorizationContext;
            _pollRepos = pollRepository;
            _policyChecker = policyChecker;
            _pollService = pollService;
        }
        public PollCreationDTO CreatePollConsole()
        {
            PollCreationDTO pollCreation = new PollCreationDTO();
            Console.WriteLine("Create your poll:\n Enter name: ");
            pollCreation.Name = Console.ReadLine();
            Console.WriteLine("Description:");
            pollCreation.Description = Console.ReadLine();
            pollCreation.OwnerId = _authorizationContext.GetLoggedUser().Id;
            Console.WriteLine("Enter Date of poll start (dd/MM/YYYY): ");
            pollCreation.LeftDateTime = Convert.ToDateTime(Console.ReadLine());
            Console.WriteLine("Enter Date of poll end (dd/MM/YYYY): ");
            pollCreation.RightDateTime = Convert.ToDateTime(Console.ReadLine());
            Console.WriteLine("Allow multiple selection? (Y/N)");
            string temp_ans = Console.ReadLine();
            Console.ReadLine();
            if (temp_ans == "Y")
                pollCreation.MultipleSelection = true;
            else
                pollCreation.MultipleSelection = false;
            return pollCreation;
        }
        public ChoiceCreationDTO CreateChoiceConsole()
        {
            ChoiceCreationDTO choiceCreation = new ChoiceCreationDTO();
            Console.WriteLine("Enter PollName to add an option:");
            string pollName1 = Console.ReadLine();
            Poll poll = _pollRepos.Get(pollName1);
            bool policyresponse = _policyChecker.CheckAdminPolicy(poll.Id);
            if (policyresponse == false)
            {
                Console.WriteLine("You have no rights to create options for this poll!");
                Console.ReadLine();
                return null;
            }
            Console.WriteLine("Enter Option Name: ");
            choiceCreation.OptionName = Console.ReadLine();
            Console.WriteLine("Enter option description: ");
            choiceCreation.OptionDescription = Console.ReadLine();
            choiceCreation.pollId = poll.Id;
            return choiceCreation;
        }
        public void ShowPollsConsole()
        {
            Console.WriteLine("Available polls: ");
            foreach (var a in _pollRepos.GetPolls())
            {
                Console.WriteLine($"{a.Name} \n {a.Description}\n Time left: {a.PollEndDate - DateTime.Now} \n");
            }
        }
        public bool PolicyCheckConsole(string poll_temp_name)
        {
            var pollPolicyFailedChecks = _pollService.CheckAllPolicy(poll_temp_name);
            if (pollPolicyFailedChecks.Count > 0)
            {
                foreach (var a in pollPolicyFailedChecks)
                {
                    Console.WriteLine(a.Value);
                    Console.ReadLine();
                }
                return false;
            }
            return true;
        }
    }
}
