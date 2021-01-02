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
using VoteSystem.Data.DTO;

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

             
            UserRepository userRepository = new UserRepository();
            ContextRegistration contextRegistration = new ContextRegistration(userRepository);
            var managePolicy = new ManagePolicy(userRepository);
            VoteRepository voteRepository = new VoteRepository();
            RegionRepository regionRepository = new RegionRepository();
            PollRepository pollRepository = new PollRepository();
            PolicyChecker policyChecker = new PolicyChecker(userRepository, contextRegistration);
            VoteService voteService = new VoteService(voteRepository, pollRepository, contextRegistration);
            PollService pollService = new PollService(contextRegistration, regionRepository, 
                                                      pollRepository, managePolicy, 
                                                      voteService, policyChecker, voteRepository);
            UserInterface userInterface = new UserInterface(userRepository, contextRegistration, pollRepository, policyChecker);
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
                    response = userRepository.UserExists(contextRegistration.GetPassportInfo().Item1,
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

                    int user_temp_id = userRepository.GetUser(contextRegistration.GetPassportInfo().Item1, contextRegistration.GetPassportInfo().Item2).Id;
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
                                PollCreationDTO pollCreation = userInterface.CreatePollConsole();
                                pollService.CreatePoll(pollCreation);
                                break;
                            #endregion
                            #region AddChoice
                            case 2:
                                ChoiceCreationDTO choiceCreation = userInterface.CreateChoiceConsole();
                                if (choiceCreation == null) 
                                {
                                    break;
                                }
                                pollService.CreateChoice(choiceCreation);
                                break;

                            #endregion
                            #region Vote
                            case 3:
                                userInterface.ShowPollsConsole();
                                Console.WriteLine("Choose the poll:");
                                string poll_temp_name = Console.ReadLine();
                                var pollPolicyFailedChecks = pollService.CheckAllPolicy(poll_temp_name);
                                if (pollPolicyFailedChecks.Count > 0)
                                {
                                    foreach (var a in pollPolicyFailedChecks)
                                    {
                                        Console.WriteLine(a.Value);
                                        Console.ReadLine();
                                    }
                                    break;
                                }

                                Poll poll1 = pollRepository.Get(poll_temp_name);
                                foreach (var a in poll1.Choices)
                                {
                                    Console.WriteLine($"{a.Name} \n {a.Description} \n");
                                }
                                Console.WriteLine("Write what you choose:");
                                List<int> allChoices = new List<int>();
                                if (poll1.MutlipleSelection == true)
                                {
                                    string option = Console.ReadLine();
                                    allChoices.Add(poll1.GetChoiceByName(option).Id);
                                    Console.WriteLine("Do you want to choose smth more? (Y/N)");
                                    string multipleResponse = Console.ReadLine();
                                    while (multipleResponse == "Y")
                                    {
                                        option = Console.ReadLine();
                                        allChoices.Add(poll1.GetChoiceByName(option).Id);
                                        Console.WriteLine("Do you want to choose smth more? (Y/N)");
                                        multipleResponse = Console.ReadLine();
                                    }
                                }
                                else
                                {
                                    string option = Console.ReadLine();
                                    allChoices.Add(poll1.GetChoiceByName(option).Id);
                                }

                                voteService.Vote(allChoices);
                                break;
                            #endregion
                            #region Policy
                            case 4:
                                userInterface.ShowPollsConsole();
                                Console.WriteLine("Enter PollName for future policy:");
                                string pollName = Console.ReadLine();

                                int? pollId = pollRepository.GetPollId(pollName);

                                if (pollId == null) {
                                    Console.WriteLine("Invalid poll name");
                                    Console.ReadLine();
                                    break;
                                }

                                bool policyresponse = policyChecker.CheckAdminPolicy(pollId.Value);

                                if (policyresponse == false)
                                {
                                    Console.WriteLine("You have no rights to give policy for this poll!");
                                    Console.ReadLine();
                                    break;
                                }
                                Console.WriteLine("Which rights do you want to give? (Admin/Access)");
                                string answer_for_rights = Console.ReadLine();
                                if(!Enum.TryParse(answer_for_rights, out PolicyType policyType)) {
                                    Console.WriteLine("Fuck you dumbass paralytic idiot who cannot type needed shit!");
                                    break;
                                }

                                Console.WriteLine("Enter email for user who you want to give policy:");
                                string email = Console.ReadLine();

                                User user = userRepository.GetUser(email);
                                managePolicy.GivePolicyToUser(user.Id, pollId.Value, policyType);                                
                                break;
                            #endregion
                            #region Results
                            case 5:
                                userInterface.ShowPollsConsole();
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