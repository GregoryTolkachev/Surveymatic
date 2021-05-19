using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Surveymatic.Shared;

namespace Surveymatic.Server.Hubs
{
    public interface ISurveyHub
    {
        Task SurveyAdded(SurveySummary survey);
        Task SurveyUpdated(Survey survey);
    }

    public class SurveyHub: Hub<ISurveyHub>
    {
        public async Task JoinSurveyGroup(Guid surveyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, surveyId.ToString());
        }
        public async Task LeaveSurveyGroup(Guid surveyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, surveyId.ToString());
        }
    }
}