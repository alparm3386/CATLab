using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace CAT.Services.Common
{
    public class UserService : IUserService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public UserService(DbContextContainer dbContextContainer, IConfiguration configuration, IMapper mapper, ILogger<UserService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Client> GetClient(int clientId)
        {
            var client = await _dbContextContainer!.MainContext!.Clients!
                .Include(client => client.Company)
                .FirstOrDefaultAsync(client => client.Id == clientId);

            var user = await _dbContextContainer.IdentityContext.Users.Where(u => u.Id == client!.UserId).FirstOrDefaultAsync();
            client!.User = user!;

            return client;
        }
    }
}
