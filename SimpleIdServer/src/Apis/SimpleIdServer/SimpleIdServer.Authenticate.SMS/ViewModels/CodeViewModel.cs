using System; 
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SimpleIdServer.Authenticate.SMS.ViewModels
{
    public class CodeViewModel
    { 
        public const string RESEND_ACTION = "resend"; 
        public const string SUBMIT_ACTION = "submit"; 


        public string Code { get; set; } 
        public string AuthRequestCode { get; set; } 
        public string ClaimName { get; set; } 
        public string ClaimValue { get; set; } 
        public string Action { get; set; } 


        public void Validate(ModelStateDictionary modelState)
        { 
            if (modelState == null) 
            { 
                throw new ArgumentNullException(nameof(modelState)); 
            } 


            if (Action == RESEND_ACTION) 
            { 
                if (string.IsNullOrWhiteSpace(ClaimValue)) 
                { 
                    modelState.AddModelError("ClaimValue", "The claim must be specified"); 
                } 
            } 


            if (Action == SUBMIT_ACTION) 
            { 
                if (string.IsNullOrWhiteSpace(Code)) 
                { 
                    modelState.AddModelError("Code", "The confirmation code must be specified"); 
                } 
            } 
        } 
    }
}
