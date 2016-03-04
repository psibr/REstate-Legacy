using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REstate.Auth.Repositories;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Auth.Susanoo
{
    public class AuthRepositoryContextFactory
        : IAuthRepositoryContextFactory
    {
        public IAuthRepository OpenAuthRepositoryContext()
        {
            return new AuthRepository(new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")));
        }
    }
}
