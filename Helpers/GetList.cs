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

		public GetList(string sentlist, string list)
			{
			contactHow = sentlist;
			listoffaculty = list;
			}
			public async Task StartAsync(IDialogContext context)
			{
				var message = "Great! You selected: " + contactHow + " .";
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
				TwilioProgram.SendSms(phone, listoffaculty);
				}
				else
				{
				await context.PostAsync("Sorry, there was something with the phone number you sent: " + phone);
				context.Done(this);
				}

			}
		
		
	}
}