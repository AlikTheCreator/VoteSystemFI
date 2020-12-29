using Ninject;
using System;
using VoteSystem.Data.Repositories;
using VoteSystem.Domain.Interfaces;
using VoteSystem.Domain.DefaultImplementations;
using VoteSystem.EF.Repositories;
using VoteSystem.Data.Entities.UserPolicyAggregate;
using VoteSystem.Data.Entities.PollAggregate;
using System.Collections.Generic;
using System.Linq;

namespace VoteSystem.Cosnole
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel();
            
            #region BindingOnly
            kernel.Bind<IPollRepository>().To<PollRepository>();
            kernel.Bind<IRegionRepository>().To<RegionRepository>();
            kernel.Bind<IUserRepository>().To<UserRepository>();
            kernel.Bind<IVoteRepository>().To<VoteRepository>();

            kernel.Bind<IManagePolicy>().To<ManagePolicy>();
            kernel.Bind<IPolicyChecker>().To<PolicyChecker>();
            kernel.Bind<IPollService>().To<PollService>();
            kernel.Bind<IRegistrationUserService>().To<RegistrationUserService>();
            kernel.Bind<IVoteService>().To<VoteService>();

            ContextRegistration contextRegistration = new ContextRegistration();
            UserRepository userRepository = new UserRepository();
            VoteRepository voteRepository = new VoteRepository();
            RegionRepository regionRepository = new RegionRepository();
            PollRepository pollRepository = new PollRepository();
            UserService userService = new UserService(userRepository);
            PollService pollService = new PollService(userRepository, regionRepository, pollRepository);
            PolicyChecker policyChecker = new PolicyChecker(userService);
            VoteService voteService = new VoteService(voteRepository, pollRepository);
            var managePolicy = new ManagePolicy(userRepository);
            IRegistrationUserService registrationUserService = new RegistrationUserService(contextRegistration,
                                                                                           voteRepository,
                                                                                           regionRepository,
                                                                                           userRepository);
            #endregion
            while (true)
            {
                Console.Clear();
                System.Console.WriteLine($"Hello, Person. Here's some service for you to make your own choice for the future of your country \n" +
                $"Please enter your passport data to verify your identity:");
                string passport = Console.ReadLine();
                Console.WriteLine("Now identification code:");
                int indefcode = Int32.Parse(Console.ReadLine());
                contextRegistration.SetPasswordInfo(passport, indefcode);
                //int user_temp_id = userService.GetUserByMainInfo(contextRegistration.GetPassportInfo().Item1, contextRegistration.GetPassportInfo().Item2).Id;
                bool response;
                try
                {
                    response = registrationUserService.ValidateUser(contextRegistration.GetPassportInfo().Item1,
                                              contextRegistration.GetPassportInfo().Item2);
                }
                catch (Exception)
                {
                    Console.WriteLine("We don't have information about you");
                    Console.ReadLine();
                    response = false;
                }
                if (response == false)
                {
                    Console.WriteLine("Sorry, but you are not allowed to vote");
                }
                else
                {
                    bool choice = true;

                    int user_temp_id = userService.GetUserByMainInfo(contextRegistration.GetPassportInfo().Item1, contextRegistration.GetPassportInfo().Item2).Id;
                    while (choice) { 
                    Console.Clear();
                    Console.WriteLine("Welcome to our service!");
                    Console.WriteLine("Select option:\n" +
                        "1. Create Poll;\n" +
                        "2. Add Choice to Poll;\n" +
                        "3. Vote;\n" +
                        "4. Give Policy;\n"+
                        "5. Show poll results;\n"+
                        "0. Exit this shit;");
                    var answer = Int32.Parse(Console.ReadLine());
                        switch (answer)
                        {
                            #region Create Poll
                            case 1:
                                Console.WriteLine("Create your poll:\n Enter name: ");
                                string name = Console.ReadLine();
                                Console.WriteLine("Description:");
                                string desc = Console.ReadLine();
                                int ownerpollId = userRepository.GetUser(contextRegistration.GetPassportInfo().Item1,
                                                       contextRegistration.GetPassportInfo().Item2).Id;
                                Console.WriteLine("Enter Date of poll start (dd/MM/YYYY): ");
                                var start = Convert.ToDateTime(Console.ReadLine());
                                Console.WriteLine("Enter Date of poll end (dd/MM/YYYY): ");
                                var end = Convert.ToDateTime(Console.ReadLine());
                                Console.WriteLine("Allow multiple selection? (Y/N)");
                                bool multiple;
                                string temp_ans = Console.ReadKey().ToString();
                                Console.ReadLine();
                                if (temp_ans == "Y")
                                    multiple = true;
                                else
                                    multiple = false;
                                int creation_response = pollService.CreatePoll(name, desc, ownerpollId, start, end, multiple);
                                managePolicy.GiveAdminPolicyToUser(ownerpollId, creation_response);
                                break;
                            #endregion

                            #region AddChoice
                            case 2:
                                Console.WriteLine("Enter PollName to add an option:");
                                string pollName1 = Console.ReadLine();
                                Poll poll1 = pollService.GetPoll(pollName1);
                                bool policyresponse1 = policyChecker.CheckAdminPolicy(user_temp_id, poll1.Id);
                                if (policyresponse1 == false)
                                {
                                    Console.WriteLine("You have no rights to create options for this poll!");
                                    Console.ReadLine();
                                    break;
                                }
                                Console.WriteLine("Enter Option Name: ");
                                string optionName = Console.ReadLine();
                                Console.WriteLine("Enter option description: ");
                                string descr = Console.ReadLine();
                                pollService.CreateChoice(optionName, descr, poll1.Id);
                                break;

                            #endregion

                            #region Vote
                            case 3:
                                Console.WriteLine("Available polls: ");
                                foreach (var a in pollService.ShowAllPolls())
                                {
                                    Console.WriteLine($"{a.Name} \n {a.Description}\n Time left: {a.PollEndDate - DateTime.Now} \n");
                                }
                                Console.WriteLine("Choose the poll:");
                                string poll_temp_name = Console.ReadLine(); 
                                Poll poll2 = pollService.GetPoll(poll_temp_name);
                                bool policyresponse2 = policyChecker.CheckPolicy(user_temp_id, poll2.Id);
                                bool multiplevoteresponse = voteService.CheckIfVoted(user_temp_id, poll_temp_name);

                                if (multiplevoteresponse == true)
                                {
                                    Console.WriteLine("You have already voted");
                                    Console.ReadLine();
                                    break;
                                }
                                if (policyresponse2 == false)
                                {
                                    Console.WriteLine("Sorry, but you cannot vote!");
                                    Console.ReadLine();
                                    break;
                                }
                                foreach (var a in pollService.GetChoices(poll_temp_name))
                                {
                                    Console.WriteLine($"{a.Name} \n {a.Description} \n");
                                }
                                Console.WriteLine("Write what you choose:");
                                List<string> allChoices = new List<string>();
                                if (poll2.MutlipleSelection == true)
                                {
                                    string option = Console.ReadLine();
                                    allChoices.Add(option);
                                    Console.WriteLine("Do you want to choose smth more? (Y/N)");
                                    string multipleResponse = Console.ReadLine();
                                    while (multipleResponse == "Y")
                                    {
                                        option = Console.ReadLine();
                                        allChoices.Add(option);
                                        Console.WriteLine("Do you want to choose smth more? (Y/N)");
                                        multipleResponse = Console.ReadLine();
                                    }
                                }
                                else
                                {
                                    string option = Console.ReadLine();
                                    allChoices.Add(option);
                                }
                                foreach (var a in allChoices)
                                {
                                    voteService.Vote(user_temp_id, pollService.GetChoices(poll_temp_name).FirstOrDefault(c => c.Name == a).Id);                                }
                                break;
                            #endregion
                            #region Policy
                            case 4:
                                
                                Console.WriteLine("Enter PollName for future policy:");
                                string pollName = Console.ReadLine();
                                Poll poll = pollService.GetPoll(pollName);
                                bool policyresponse = policyChecker.CheckAdminPolicy(user_temp_id, poll.Id);
                                if (policyresponse == false)
                                {
                                    Console.WriteLine("You have no rights to give policy for this poll!");
                                    Console.ReadLine();
                                    break;
                                }
                                Console.WriteLine("Which rights do you want to give? (Admin/Access)");
                                string answer_for_rights = Console.ReadLine();
                                if (answer_for_rights == "Admin")
                                {
                                    Console.WriteLine("Enter email for user who you want to give policy:");
                                    string email = Console.ReadLine();
                                    User user = userService.GetUserByEmail(email);
                                    managePolicy.GiveAdminPolicyToUser(user.Id, poll.Id);
                                }
                                else if (answer_for_rights == "Access")
                                {
                                    Console.WriteLine("Enter email for user who you want to give policy:");
                                    string email = Console.ReadLine();
                                    User user = userService.GetUserByEmail(email);
                                    managePolicy.GivePolicyToUser(user.Id, poll.Id);
                                }
                                else
                                {
                                    Console.WriteLine("Fuck you dumbass paralytic idiot who cannot type needed shit!");
                                }
                                break;
                            #endregion
                            #region Results
                            case 5:
                                Console.WriteLine("Available polls: ");
                                foreach (var a in pollService.ShowAllPolls())
                                {
                                    Console.WriteLine($"{a.Name} \n {a.Description}\n Time left: {a.PollEndDate - DateTime.Now} \n");
                                }
                                Console.WriteLine("Enter Pollname to see the results:");
                                string pollResultName = Console.ReadLine();
                                foreach (var a in voteService.GetPollResult(pollResultName))
                                {
                                    Console.WriteLine($"{a.Key.ToString()}  - {a.Value.ToString()} ");
                                }
                                Console.ReadLine();
                                break;
                            #endregion
                            #region Exit
                            case 0:
                                choice = false;
                                break;
                            #endregion
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
