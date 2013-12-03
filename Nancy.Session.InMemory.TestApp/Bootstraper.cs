using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nancy.Session.InMemory.TestApp {
    public class Bootstraper : DefaultNancyBootstrapper {
        protected override void ApplicationStartup(TinyIoc.TinyIoCContainer container, Bootstrapper.IPipelines pipelines) {
            base.ApplicationStartup(container, pipelines);

            MemoryCacheBasedSessions.Enable(pipelines, new TimeSpan(0, 0, 10));
        }
    }
}
