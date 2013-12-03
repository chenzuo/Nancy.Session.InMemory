using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using Nancy.Bootstrapper;

//TODO: check if possible to set cache item value to NULL
//TODO: check removal of cache item
//TODO: check cache item expiration time
namespace Nancy.Session.InMemory {
    /// <summary>
    /// MemoryCache based session storage
    /// </summary>
    public class MemoryCacheBasedSessions {
        /// <summary>
        /// Cache storage
        /// </summary>
        ConcurrentDictionary<string, MemoryCache> storage;

        /// <summary>
        /// Entry life-time span
        /// </summary>
        TimeSpan entrySpan;

        /// <summary>
        /// Cookie name for storing session information
        /// </summary>
        static string cookieName = "_nc";

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheBasedSessions"/> class.
        /// </summary>
        public MemoryCacheBasedSessions(TimeSpan entryLifespan) {
            entrySpan = entryLifespan;
            storage = new ConcurrentDictionary<string, MemoryCache>();
        }

        /// <summary>
        /// Gets the cookie name that the session is stored in
        /// </summary>
        /// <returns>Cookie name</returns>
        public static string GetCookieName() {
            return cookieName;
        }

        /// <summary>
        /// Initialize and add session hooks to the application pipeline
        /// </summary>
        /// <param name="pipelines">Application pipelines</param>
        /// <param name="entryLifespan">Session entry life-time value</param>
        /// <returns>Initialized MemoryCache session storage</returns>
        public static MemoryCacheBasedSessions Enable(IPipelines pipelines, TimeSpan entryLifespan) {
            var sessionStore = new MemoryCacheBasedSessions(entryLifespan);

            pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx => LoadSession(ctx, sessionStore));
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => SaveSession(ctx, sessionStore));

            return sessionStore;
        }

        /// <summary>
        /// Initialize storage with default entry life-time (10 minutes) and add session hooks to the application pipeline
        /// </summary>
        /// <param name="pipelines">Application pipelines</param>
        /// <returns>Initialized MemoryCache session storage</returns>
        public static MemoryCacheBasedSessions Enable(IPipelines pipelines) { 
            return Enable(pipelines, TimeSpan.FromMinutes(10));
        }

        /// <summary>
        /// Save the session into the response
        /// </summary>
        /// <param name="session">Session to save</param>
        /// <param name="response">Response to save into</param>
        public void Save(Request request, Response response) {
            var session = request.Session;
            if (session == null || !session.HasChanged) { return; }

            string id;
            if (request.Cookies.ContainsKey(cookieName)) {
                id = request.Cookies[cookieName];
            } else {
                id = Guid.NewGuid().ToString();
                response.AddCookie(cookieName, id);
            }

            var cache = storage.GetOrAdd(id, new MemoryCache(id));
            foreach (var kvp in session) {
                if (cache.Contains(kvp.Key)) { cache.Remove(kvp.Key); }
                cache.Add(kvp.Key, kvp.Value, DateTime.Now.Add(entrySpan));
            }
        }

        /// <summary>
        /// Saves the request session into the response
        /// </summary>
        /// <param name="context">Nancy context</param>
        /// <param name="sessionStore">Session store</param>
        private static void SaveSession(NancyContext context, MemoryCacheBasedSessions sessionStore) {
            sessionStore.Save(context.Request, context.Response);
        }

        /// <summary>
        /// Loads the session from the request
        /// </summary>
        /// <param name="request">Request to load from</param>
        /// <returns>ISession containing the load session values</returns>
        public ISession Load(Request request) {
            var session = new Session();
            if (request.Cookies.ContainsKey(cookieName)) {
                var id = request.Cookies[cookieName];
                MemoryCache cache;
                if (storage.TryGetValue(id, out cache)){
                    foreach(var item in cache){
                        session[item.Key] = item.Value;
                    }
                }
            }

            return session;
        }

        /// <summary>
        /// Loads the request session
        /// </summary>
        /// <param name="context">Nancy context</param>
        /// <param name="sessionStore">Session store</param>
        /// <returns>Always returns null</returns>
        private static Response LoadSession(NancyContext context, MemoryCacheBasedSessions sessionStore) {
            if (context.Request == null) { return null; }

            context.Request.Session = sessionStore.Load(context.Request);

            return null;
        }
    }
}
