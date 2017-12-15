using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SimpleEchoBot.Helpers
{
	public class TwilioProgram
	{
		public static void SendSms(string phone, string listoffaculty)
		{
			SendIt(phone,listoffaculty).Wait();
				Console.Write("Press any key to continue.");
				Console.ReadKey();
			}
		static async Task SendIt(string phone, string listoffaculty)
		{
			// Your Account SID from twilio.com/console
			var accountSid = "ACb34e894058b373fb4603720f5579d222";
			// Your Auth Token from twilio.com/console
			var authToken = "af3cb7900f8a6233e53fdb4bac6b3784";

			TwilioClient.Init(accountSid, authToken);
			
				var message = await MessageResource.CreateAsync(
			    to: new PhoneNumber("+1" + phone),
			    from: new PhoneNumber("+17742983548"),
			    body: listoffaculty);

			Console.WriteLine(message.Sid);
		}
		
		
	}
}