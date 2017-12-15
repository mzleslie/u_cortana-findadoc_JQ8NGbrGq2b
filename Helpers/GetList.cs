using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Helpers.BotPromptDialog
{
	[Serializable]
	public class GetList : IDialog<object>
	{
			string phone;
			string contactHow;
			string listoffaculty;
			string howmany;
		int thismany;
		string[] listarray = new string[] { };

		public GetList(string sentlist, string list)
			{
			contactHow = sentlist;
			listoffaculty = list;
		}
			public async Task StartAsync(IDialogContext context)
			{
			
			var message = "";
			bool isnumberonly = Int32.TryParse(contactHow, out thismany);



				//this is contact methos

				message = "Great! You selected: " + contactHow + " .";
				switch (contactHow.ToLower())
				{
					case "email":
						message = "Unfortunately EMAIL is not set up at this time. ";
						await context.PostAsync(message);
						context.Done(this);
						break;
					case "sms":
						message += "Please provide your cell phone number.";
						await context.PostAsync(message);
						context.Wait(this.MessageReceivedAsync);
						break;
					case "slack":
						message = "Sorry, SLACK is not set up, it will be soon.";
						await context.PostAsync(message);
						context.Done(this);
						break;

				}
		

			}

		private Task SendListTask(IDialogContext context, IAwaitable<object> result)
		{
			throw new NotImplementedException();
		}

		public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> phonenumber)
			{
				var response = await phonenumber;
			//get numbers only
			     Regex digitsOnly = new Regex(@"[^\d]");
				phone =  digitsOnly.Replace(response.Text, "");
			
				if (!string.IsNullOrEmpty(phone))
			{
				await context.PostAsync("You will get a text shortly with the results at the following number: " + phone);
				context.Done(this);
				try
				{
					TwilioProgram.SendSms(phone, listoffaculty);
				}
				catch (Exception ex)
				{
					await context.PostAsync("Sorry, there was something with the phone number you sent: " + phone + ". I can only send to 774-239-0217 during this trial period. " + Environment.NewLine + Environment.NewLine + ex.Message + "Can I help you find someone else?");
					context.Done(this);
				}
				await context.PostAsync("Can I help you find someone else?");
				context.Done(this);
			}
				else
				{
				await context.PostAsync("Sorry, there was something with the phone number you sent: " + phone);
				context.Done(this);
				}

			}
		
		
	}
}