using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace CAT.Services.Common
{
    public class UserService : IUserService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IdentityDbContext _identityDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public UserService(DbContextContainer dbContextContainer, IdentityDbContext identityDbContext, 
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration, IMapper mapper, ILogger<UserService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _identityDbContext = identityDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
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

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }
    }
}
