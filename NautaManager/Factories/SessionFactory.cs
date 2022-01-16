using NautaManager.Models;
using NautaManager.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NautaManager.Factories
{
    static class SessionFactory
    {
        public async static Task<UserSession> LoadSessionAsync(UserSessionStore store)
        {
            var model = new UserSession(store.CSRFHW, store.ATTRIBUTE_UUID, store.wlanuserip)
            {
                Username = store.Username,
                Status = store.Status ?? new AccountStatus(),
                IsActive = store.ATTRIBUTE_UUID != null
            };
            model.InitClientSession();
            if (model.ATTRIBUTE_UUID == null)
                await model.InitSession();
            model.LoadPassword(store.Password);
            return model;
        }

        public static UserSessionStore CastSessionToStore(UserSession session)
        {
            var model = new UserSessionStore()
            {
                Username = session.Username,
                Password = session.GetHashedPassword(),
                Status = session.Status,
                CSRFHW = session.CSRFHW,
                ATTRIBUTE_UUID = session.ATTRIBUTE_UUID,
                wlanuserip = session.wlanuserip
            };
            session.SaveHTTPSession();
            return model;
        }
    }
}
