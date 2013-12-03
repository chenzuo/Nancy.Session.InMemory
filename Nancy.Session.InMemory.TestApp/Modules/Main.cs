using System;
using System.Collections.Concurrent;
using Nancy.Bootstrapper;

namespace Nancy.Session.InMemory {
    public class Main : NancyModule {
        public Main() {
            Get["/"] = OnGetMain;
            Post["/"] = OnPostMain;
        }

        dynamic OnGetMain(dynamic args) {
            dynamic model = new DynamicDictionary();
            if (Session["Message"] != null) {
                model.HasMessage = true;
                model.Message = Session["Message"];
            } else {
                model.HasMessage = false;
            }
            return View["Main", model];
        }

        dynamic OnPostMain(dynamic args) {
            var message = Request.Form.message;
            if (message.HasValue && !string.IsNullOrWhiteSpace(message)) { Session["Message"] = message.Value; }
            return Response.AsRedirect("~/");
        }
    }
}