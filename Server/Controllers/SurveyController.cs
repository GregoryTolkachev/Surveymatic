using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Surveymatic.Server.Hubs;
using Surveymatic.Shared;

namespace Surveymatic.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly IHubContext<SurveyHub, ISurveyHub> hubContext;
        private static ConcurrentBag<Survey> surveys = new ConcurrentBag<Survey> {
          new Survey {
              Id = Guid.Parse("b00c58c0-df00-49ac-ae85-0a135f75e01b"),
              Title = "Which of the following covid vaccines you used ?",
              ExpiresAt = DateTime.Now.AddMinutes(10),
              Options = new List<string>{ "Pfizer", "Moderna", "AstraZeneca", "Sputnik", "None of the above" },
              Answers = new List<SurveyAnswer>{
                new SurveyAnswer { Option = "Pfizer" },
                new SurveyAnswer { Option = "Moderna" },
                new SurveyAnswer { Option = "AstraZeneca" },
                new SurveyAnswer { Option = "Sputnik" },
                new SurveyAnswer { Option = "Vodka" },
                new SurveyAnswer { Option = "Too young" },
                new SurveyAnswer { Option = "Don't want any" },
                new SurveyAnswer { Option = "None of the above" }
              }
          },

          new Survey {
              Id = Guid.Parse("7e467e51-9999-427e-bf81-015076b9f24c"),
              Title = "Where did you work during covid lockdown?",
              ExpiresAt = DateTime.Now.AddMinutes(5),
              Options = new List<string>{ "Home", "Workplace", "Student", "Didn't work", "Retired" },
              Answers = new List<SurveyAnswer>{
                new SurveyAnswer { Option = "Home" },
                new SurveyAnswer { Option = "Workplace" },
                new SurveyAnswer { Option = "Student" },
                new SurveyAnswer { Option = "Didn't work" },
                new SurveyAnswer { Option = "Retired" },
                new SurveyAnswer { Option = "Cottage" },
                new SurveyAnswer { Option = "B" },
                new SurveyAnswer { Option = "Moms" },
                new SurveyAnswer { Option = "Dads" },
                new SurveyAnswer { Option = "Girlfriends" }
              }
          },
        };

        public SurveyController(IHubContext<SurveyHub, ISurveyHub> surveyHub)
        {
            this.hubContext = surveyHub;
        }

        [HttpGet()]
        public IEnumerable<SurveySummary> GetSurveys()
        {
            return surveys.Select(s => s.ToSummary());
        }

        [HttpGet("{id}")]
        public ActionResult GetSurvey(Guid id)
        {
            var survey = surveys.SingleOrDefault(t => t.Id == id);
            if (survey == null) return NotFound();

            return new JsonResult(survey);
        }


        [HttpPut()]
        public async Task<Survey> AddSurvey([FromBody]AddSurveyModel addSurveyModel)
        {
            var survey = new Survey{
              Title = addSurveyModel.Title,
              ExpiresAt = DateTime.Now.AddMinutes(addSurveyModel.Minutes.Value),
              Options = addSurveyModel.Options.Select(o => o.OptionValue).ToList()
            };

            surveys.Add(survey);
            await this.hubContext.Clients.All.SurveyAdded(survey.ToSummary());
            return survey;
        }

        [HttpPost("{surveyId}/answer")]
        public async Task<ActionResult> AnswerSurvey(Guid surveyId, [FromBody]SurveyAnswer answer)
        {
            var survey = surveys.SingleOrDefault(t => t.Id == surveyId);
            if (survey == null) return NotFound();
            if (((IExpirable)survey).IsExpired) return StatusCode(400, "This survey has expired");


            survey.Answers.Add(new SurveyAnswer{
              SurveyId = surveyId,
              Option = answer.Option
            });


            await this.hubContext.Clients.Group(surveyId.ToString()).SurveyUpdated(survey);

            return new JsonResult(survey);
        }
    }
}