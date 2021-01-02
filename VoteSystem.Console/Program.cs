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
using Microsoft.Extensions.DependencyInjection;

namespace VoteSystem.Cosnole
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = new ServiceCollection();
            collection.AddScoped<IPollRepository, PollRepository>();

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
            var managePolicy = new ManagePolicy(userRepository);
            VoteRepository voteRepository = new VoteRepository();
            RegionRepository regionRepository = new RegionRepository();
            PollRepository pollRepository = new PollRepository();
            PolicyChecker policyChecker = new PolicyChecker(userRepository);
            VoteService voteService = new VoteService(voteRepository, pollRepository);
            PollService pollService = new PollService(userRepository, regionRepository, 
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
                                ChoiceCreationDTO choiceCreation = userInterface.CreateChoiceConsole(user_temp_id);
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
                                Poll poll1 = pollRepository.Get(poll_temp_name);
                                if (pollService.CheckAllPolicy(poll_temp_name, user_temp_id).Count > 0)
                                {
                                    foreach (var a in pollService.CheckAllPolicy(poll_temp_name, user_temp_id))
                                    {
                                        Console.WriteLine(a.Value);
                                        Console.ReadLine();
                                    }
                                    break;
                                }
                                foreach (var a in pollRepository.GetChoices(poll_temp_name))
                                {
                                    Console.WriteLine($"{a.Name} \n {a.Description} \n");
                                }
                                Console.WriteLine("Write what you choose:");
                                List<string> allChoices = new List<string>();
                                if (poll1.MutlipleSelection == true)
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
                                    voteService.Vote(user_temp_id, pollRepository.GetChoices(poll_temp_name).FirstOrDefault(c => c.Name == a).Id);                                }
                                break;
                            #endregion
                            #region Policy
                            case 4:
                                userInterface.ShowPollsConsole();
                                Console.WriteLine("Enter PollName for future policy:");
                                string pollName = Console.ReadLine();
                                Poll poll = pollRepository.Get(pollName);
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
                                    User user = userRepository.GetUser(email);
                                    managePolicy.GiveAdminPolicyToUser(user.Id, poll.Id);
                                }
                                else if (answer_for_rights == "Access")
                                {
                                    Console.WriteLine("Enter email for user who you want to give policy:");
                                    string email = Console.ReadLine();
                                    User user = userRepository.GetUser(email);
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