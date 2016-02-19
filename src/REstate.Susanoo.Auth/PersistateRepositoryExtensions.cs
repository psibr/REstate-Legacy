using System;
using REstate.Auth.Repositories;
using REstate.Repositories;

namespace REstate.Susanoo.Auth
{
    public static class REstateRepositoryExtensions
    {
        public static IAuthRepository GetAuthRepository(this IRepository repository)
        {
            var root = repository as Repository;

            if(root == null) throw new ArgumentException("Type mismatch between root repository and auth library.", nameof(repository));

            return new AuthRepository(root);
        }
    }
}