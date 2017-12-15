using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace SimpleEchoBot.Helpers.ProfilesApiClient
{

	public class FacultyList
	{
		public enum SendList
		{
			SMS,
			Email,
			Slack,
		}
		public class FacultyPerson
		{
			public string PersonId { get; set; }
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string DisplayName { get; set; }
			public string InstitutionName { get; set; }
			public string Department { get; set; }
			public string Division { get; set; }
			public string FacultyTitle { get; set; }
			public string ProfileImage { get; set; }
			//public string ProfileImageBinary { get; set; }
			public string ShowImage { get; set; }
			public string Narrative { get; set; }
			public string ProfilePersonUrl { get; set; }
			public List<PersonDegree> PersonDegrees { get; set; }
			//public string AffiliationPrimary { get; set; }
			public string AffiliationSecondary { get; set; }
		}


		public class PersonDegree
		{
			public string Degree { get; set; }
			public string Institute { get; set; }
		}

		public class ServiceInfo
		{
			public List<FacultyPerson> faculty { get; set; }
			public string totalCount { get; set; }
			public string orderBy { get; set; }
		}


		public class ProfileFacultyReader
		{
			public static ServiceInfo GetServiceInfo(string jstr = "")
			{
				try
				{
					JObject serviceInfoJson = getObjectsFromJson(jstr);
					var serviceInfo = CreateServiceInfoObj(serviceInfoJson);
					return serviceInfo;
				}
				catch (Exception ex)
				{
					var eventError = new ServiceInfo()
					{
						faculty = new List<FacultyPerson>(){new FacultyPerson()
					    {
						   PersonId = "0",
						   FirstName = "no results were found",
						   LastName = "",
						   DisplayName = "there was an error getting the list of faculty",
						   InstitutionName = "",
						   Department = "",
						   Division = "",
						   FacultyTitle = ex.Message,
						   ProfileImage = "",
						   //ProfileImageBinary = "",
						   ShowImage = "",
						   ProfilePersonUrl = "",
						   //AffiliationPrimary = ""
					    }}
					};

					return eventError;
				}
			}

			public static ServiceInfo CreateServiceInfoObj(JObject serviceInfoJson)
			{
				var serviceInfo = new ServiceInfo();
			     serviceInfo.faculty = serviceInfoJson["value"].Select(p => new FacultyPerson()
					{
						PersonId = (p["PersonId"] != null) ? p["PersonId"].ToString() : "",
						FirstName = (p["FirstName"] != null) ? p["FirstName"].ToString() : "",
						LastName =  (p["LastName"] != null) ? p["LastName"].ToString() : "",
						DisplayName = (p["DisplayName"] != null) ? p["DisplayName"].ToString() : "",
					//InstitutionName = p["institutionname"].ToString(),
						Department = (p["Department"] != null) ? p["Department"].ToString() : "",
					     Division = (p["Division"] != null) ? p["Division"].ToString() + " in " : "",
					FacultyTitle = (p["FacultyTitle"] != null) ? p["FacultyTitle"].ToString() : "",
					//ShowImage = p["showPhoto"].ToString(),v
					//ProfileImage = (p["photo"] != null) ? p["photo"].ToString() : "",
					//ProfileImageBinary = "",
					ProfilePersonUrl = (p["ProfilePersonUrl"] != null) ? p["ProfilePersonUrl"].ToString() : ""
					//AffiliationPrimary = GetProfileAffiliation(p["links"][3]["url"].ToString(), p["personid"].ToString()),
					//PersonDegrees = p["education"].Select(i => new PersonDegree()
					//{
					//Degree = i["degree"].ToString(),
					//Institute = i["institute"].ToString()
					//}).ToList()
				}).ToList();
			
				return serviceInfo;
			}
		

			public static JObject getObjectsFromJson(string jsonStr)
			{
				var objs = JObject.Parse(jsonStr);

				Console.Write(objs);
				return objs;
			}
			


		}
	}
}