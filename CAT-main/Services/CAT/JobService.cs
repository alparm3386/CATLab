﻿using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CATService;
using CAT.Controllers.Api;
using CAT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;

namespace CAT.Services.CAT
{
    public class JobService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;


        public JobService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector, 
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _logger = logger;
            _mapper = mapper;
        }

        public void ProcessJob(int idJob)
        {
            _catConnector.ParseDoc(idJob);
        }

        public FileData CreateDocument(int idJob)
        {
            return _catConnector.CreateDoc(idJob, Guid.NewGuid().ToString(), false);
        }
    }
}
