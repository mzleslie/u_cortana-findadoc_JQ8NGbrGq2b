using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
using System.Web.UI;
using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
	using System.Net.Http;
	using System.Net.Http.Headers;
using SimpleEchoBot.Helpers.ProfilesApiClient;
using static SimpleEchoBot.Helpers.ProfilesApiClient.FacultyList;
using SimpleEchoBot.Helpers.BotPromptDialog;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
	[LuisModel("75fc1c34-5414-465f-a565-d23f7fdb8c97", "b64a7b3b7b6649109ccd1bc850758554", LuisApiVersion.V2, domain: "eastus.api.cognitive.microsoft.com")]

	[Serializable]
    public class EchoDialog : LuisDialog<object>
    {
		 private const string EntityFacultyRank = "facultyrank";

        private const string EntityName = "name";

        private const string EntityDepartment = "department";
		private const string apiUrl = "https://profiles.search.windows.net/indexes/documentdb-index/docs?api-version=2016-09-01";
		private string facultyList = string.Empty;
		private string[] facultyListArray = new string[] { };

		[LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
		{
			string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";
			if (result.Query.ToString().ToLower() == "thank you" || result.Query.ToString().ToLower() == "thanks") {
				message = "You're welcome";
			}

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

		[LuisIntent("faqs")]
		public async Task faqs(IDialogContext context, LuisResult result)
		{

			await context.PostAsync("Let me check on that ... ");


			EntityRecommendation quantity;
			EntityRecommendation facultyrank;
			bool fr = result.TryFindEntity("facultyrank", out facultyrank);

			if (result.TryFindEntity("quantity", out quantity))
			{
				if ((quantity.Entity.Contains("many") || quantity.Entity.Contains("number")) && facultyrank.Entity != null)
				{
					var removeS = facultyrank.Entity.TrimEnd('s');

					string rs;
					var buildQueryStr = "&$filter=FacultyTitle eq '" + FirstLetterToUpper(removeS) + "'";

					using (var client = new HttpClient())
					{
						client.DefaultRequestHeaders.Add("ContentType", "application/json");
						client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
						client.DefaultRequestHeaders.Add("api-key", "0EB1DBB48C54519A69BC0BB3ACCA2AE8");
						client.DefaultRequestHeaders.Add("accept", "application/json");

						var apiUrl = "https://profiles.search.windows.net/indexes/documentdb-index/docs?api-version=2016-09-01";
						//buildQueryStr = "&searchFields=DisplayName,FirstName,LastName&$searchMode=all&search=jones";
						var response = client.GetAsync(apiUrl + buildQueryStr).Result;

						response.EnsureSuccessStatusCode();
						//rs = await response.Content.ReadAsAsync<string>();
						using (HttpContent content = response.Content)
						{
							// ... Read the string.
							Task<string> resultstring = content.ReadAsStringAsync();
							rs = resultstring.Result;
						}

						var serviceInfo = FacultyList.ProfileFacultyReader.GetServiceInfo(rs);

						if (serviceInfo != null && serviceInfo.faculty.Any())
						{
							var isAre = (serviceInfo.faculty.Count() == 1) ? " is " : " are ";
							var singularPlural = (serviceInfo.faculty.Count() == 1) ? removeS + "." : removeS + "s.";
							await context.PostAsync("There" + isAre + serviceInfo.faculty.Count() + " " + singularPlural);


						}
						else
						{
							await context.PostAsync("I didn't find any results matching your request.");
						}
					}


				}
			}
		}

		[LuisIntent("findByRankNameDepartment")]
        public async Task findByRankNameDepartment(IDialogContext context, LuisResult result)
        {
			await context.PostAsync("Searching ... ");
			

			var whatToSearchForMessage = string.Empty;
			var buildQueryStr = string.Empty;
			string rs;


			EntityRecommendation name;
			if (result.TryFindEntity("name", out name))
			{
				whatToSearchForMessage += " Name: " + name.Entity;
				buildQueryStr += "&$searchMode=all&searchFields=DisplayName,FirstName,LastName&search=" + FirstLetterToUpper(name.Entity);
			}
			EntityRecommendation fullname;
			if (result.TryFindEntity("FullName", out fullname))
			{
				if (!string.IsNullOrEmpty(fullname.Entity))
				{

				}

				whatToSearchForMessage += " Name: " + fullname.Entity;
				buildQueryStr += "&$searchMode=all&searchFields='DisplayName,FirstName,LastName'&search=" + FirstLetterToUpper(fullname.Entity);
			}
			//search both
			EntityRecommendation facultyrank;
			bool facultyrankExists = result.TryFindEntity("facultyrank", out facultyrank);

			EntityRecommendation department;
			bool departmentExists = result.TryFindEntity("department", out department);

			if (facultyrankExists && departmentExists)
			{
				whatToSearchForMessage += " department: " + FirstLetterToUpper(department.Entity) + " and faculty rank: " + FirstLetterToUpper(facultyrank.Entity);
				buildQueryStr += "&$filter=FacultyTitle eq '" + FirstLetterToUpper(facultyrank.Entity) + "' and Department eq '" + FirstLetterToUpper(department.Entity) + "' ";
			}
			//one or the other
			else
			{
				if (facultyrankExists)
				{
					whatToSearchForMessage += " faculty rank: " + FirstLetterToUpper(facultyrank.Entity);
					buildQueryStr += "&$filter=FacultyTitle eq '" + FirstLetterToUpper(facultyrank.Entity) + "'";
				}

				if (departmentExists)
				{
					whatToSearchForMessage += " department: " + FirstLetterToUpper(department.Entity);
					buildQueryStr += "&$filter=Department eq '" + FirstLetterToUpper(department.Entity) + "'";
				}

			}

			await context.PostAsync("Looking for: " + whatToSearchForMessage);
			var message = "Did I miss something?";
			if (!String.IsNullOrEmpty(buildQueryStr))
			{
				buildQueryStr += "&$orderby=LastName asc";
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("ContentType", "application/json");
					client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
					client.DefaultRequestHeaders.Add("api-key", "0EB1DBB48C54519A69BC0BB3ACCA2AE8");
					client.DefaultRequestHeaders.Add("accept", "application/json");

					var apiUrl = "https://profiles.search.windows.net/indexes/documentdb-index/docs?api-version=2016-09-01";
					//buildQueryStr = "&searchFields=DisplayName,FirstName,LastName&$searchMode=all&search=jones";
					var response = client.GetAsync(apiUrl + buildQueryStr).Result;

					response.EnsureSuccessStatusCode();
					//rs = await response.Content.ReadAsAsync<string>();
					using (HttpContent content = response.Content)
					{
						// ... Read the string.
						Task<string> resultstring = content.ReadAsStringAsync();
						rs = resultstring.Result;
					}

					var serviceInfo = FacultyList.ProfileFacultyReader.GetServiceInfo(rs);

					if (serviceInfo != null && serviceInfo.faculty.Any())
					{
						var singularPlural = (serviceInfo.faculty.Count() == 1) ? " result." : " results.";
						var listMessage = "I found " + serviceInfo.faculty.Count() + singularPlural;
						if (serviceInfo.faculty.Count() > 1 && serviceInfo.faculty.Count() < 10)
						{
							listMessage += " Let me get a list together...";
							await context.PostAsync(listMessage);
						}
						else
						{
							listMessage += "The list is pretty long, do you want me to show the list anyway?";
						}

						if (serviceInfo.faculty.Count() > 0)
						{
							
							var i = 0;
							var list = string.Empty;
							facultyListArray = new string[serviceIn
							foreach (var person in serviceInfo.faculty)
							{
								i++;
								var personid = person.PersonId;
								list += i + ". "  + person.DisplayName + ", " + person.FacultyTitle + Environment.NewLine + "     " + person.Division + person.Department + Environment.NewLine + "     " + person.ProfilePersonUrl + Environment.NewLine + Environment.NewLine;




							}
							//check if only one and it's an error
							if (serviceInfo.faculty[0].PersonId == "0")
							{
								message = serviceInfo.faculty[0].DisplayName + ", " + serviceInfo.faculty[0].FacultyTitle;

							} else
							{
								//display the list


								if (serviceInfo.faculty.Count() < 11)
								{
									string userName = "";
									//string email;
									var activity = context.Activity;
									if (activity.From.Name != null)
									{
										userName = activity.From.Name + ", ";
									}
									facultyList = "Here is the list of faculty you requested from FindADoc Bot:" + Environment.NewLine + list;
									message = Environment.NewLine + list;
									message += Environment.NewLine + Environment.NewLine;
									//message += userName;
									message += Environment.NewLine + "Would you like me to send this info to your email or phone?";
									await context.PostAsync($"{message}");
									// Show the list of plan  
									context.Wait(this.SendListTask);
								}
								else
								{
									message = "There were too many results to list. Pick a number and I will show just that one [this is not finished]...";
									await context.PostAsync($"{message}");
									context.Done(this);
								}
							}
						}
						else
						{
							message = "I didn't find any results matching your request.";
						}
						
					}
					else
					{
						message = "I didn't find any results matching your request.";
					}
				}

				
			}
			else
			{
				message = "Hmm, I didn't understand your request. Could you please re-phrase your question? Or type Help";


			}


			
			//await context.PostAsync($"{message}");
			//context.Wait(MessageReceived);
        }

        [LuisIntent("myHelp")]
        public async Task myHelp(IDialogContext context, LuisResult result)
        {
			var message = "This app will look up person in Profiles, you can say, "+ Environment.NewLine + Environment.NewLine + "find \"professor name John from Radiology\"," + Environment.NewLine + Environment.NewLine + "or looking for \"researcher from Neurology\".";
			if (result.Query.ToString().ToLower() == "thank you")
			{
				message = "You're welcome";
			}
			await context.PostAsync(message); //
            context.Wait(MessageReceived);
        }


		public virtual async Task DisplayOneItem(IDialogContext context, IAwaitable<IMessageActivity> activity)
		{

		}
		public virtual async Task SendListTask(IDialogContext context, IAwaitable<IMessageActivity> activity)
		{
			var message = await activity;
			var listOfSAnswers = new string[] { "yes", "ya", "sure", "yup", "alright","email","phone","ok" };

			bool answerIsYes = listOfSAnswers.Any(s => message.Text.ToLower().Contains(s));
			if (answerIsYes)
			{

				PromptDialog.Choice(
				    context: context,
				    resume: ChoiceReceivedAsync,
				    options: (IEnumerable<SendList>)Enum.GetValues(typeof(SendList)),
				    prompt: "Selection an option below if you would like to send this list to yourself",
				    retry: "Something went wrong. Please try again.",
				    promptStyle: PromptStyle.Auto
				    );
			}
			else
			{
				await context.PostAsync("Ok. I won't send it.");
				context.Done(this);
			}
		}
		public virtual async Task ChoiceReceivedAsync(IDialogContext context, IAwaitable<SendList> activity)
		{
			SendList response = await activity;
			context.Call<object>(new GetList(response.ToString(), facultyList), ChildDialogComplete);

		}
		public virtual async Task ChildDialogComplete(IDialogContext context, IAwaitable<object> response)
		{
			await context.PostAsync("Can I help you with another search?");
			context.Done(this);
		}


		public string FirstLetterToUpper(string str)
		{
			if (str == null)
				return null;

			if (str.Length > 1)
				return char.ToUpper(str[0]) + str.Substring(1);

			return str.ToUpper();
		}

	}
}
