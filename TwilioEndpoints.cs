using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Twilio.AspNet.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core.Services;

namespace TwilioFlowToUmbracoForms;

public static class TwilioEndpoints
{
    public static IUmbracoEndpointBuilderContext UseTwilioEndpoints(this IUmbracoEndpointBuilderContext builderContext)
    {
        var builder = builderContext.EndpointRouteBuilder;
        builder.MapPost("/twilio/flow-to-form", OnFlowToFormPost)
            .ValidateTwilioRequest();

        return builderContext;
    }

    private static async Task OnFlowToFormPost(
        HttpRequest request,
        IFormService formService,
        IRecordService recordService
    )
    {
        var form = await request.ReadFormAsync();
        var formName = form["form"];
        if (formName == StringValues.Empty) throw new Exception($"'form' parameter missing from form.");

        var umbracoForm = formService.Get(formName);
        if (umbracoForm == null) throw new Exception($"Umbraco Form '{formName}' not found.");

        var umbracoFormFields = new Dictionary<Guid, RecordField>();
        foreach (var key in form.Keys)
        {
            if (key == "form") continue;
            var formField = umbracoForm.AllFields.FirstOrDefault(f => f.Alias == key);
            if (formField == null) continue;
            umbracoFormFields.Add(formField.Id, new RecordField(formField)
            {
                // .Values expects List<Object>
                Values = form[key].Cast<object>().ToList()
            });
        }

        var record = new Record
        {
            Created = DateTime.Now,
            Form = umbracoForm.Id,
            RecordFields = umbracoFormFields,
            State = FormState.Submitted
        };

        recordService.Submit(record, umbracoForm);
    }
}