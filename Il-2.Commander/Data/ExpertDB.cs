﻿using Microsoft.EntityFrameworkCore;

namespace Il_2.Commander.Data
{
    class ExpertDB : DbContext
    {
        public ExpertDB()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(string.IsNullOrEmpty(SetApp.Config.ConnectionString))
            {
                SetApp.SetUp();
            }
            optionsBuilder.UseSqlServer(SetApp.Config.ConnectionString);
        }
        public virtual DbSet<AirFields> AirFields { get; set; }
        public virtual DbSet<CompTarget> CompTarget { get; set; }
        /// <summary>
        /// Направление атаки синих. Точки красные
        /// </summary>
        public virtual DbSet<DStrikeBlue> DStrikeBlue { get; set; }
        /// <summary>
        /// Направление атаки Красных. Точки синие.
        /// </summary>
        public virtual DbSet<DStrikeRed> DStrikeRed { get; set; }
        public virtual DbSet<DurationMission> DurationMission { get; set; }
        public virtual DbSet<GraphCity> GraphCity { get; set; }
        public virtual DbSet<InputsBridge> InputsBridge { get; set; }
        public virtual DbSet<MissionObj> MissionObj { get; set; }
        public virtual DbSet<PreSetupMap> PreSetupMap { get; set; }
        public virtual DbSet<TempGraphCity> TempGraphCity { get; set; }
        public virtual DbSet<ServerInputs> ServerInputs { get; set; }
        public virtual DbSet<MTimer> MTimer { get; set; }
        public virtual DbSet<ProfileUser> ProfileUser { get; set; }
        public virtual DbSet<LinkedAccount> LinkedAccount { get; set; }
        public virtual DbSet<OnlinePilots> OnlinePilots { get; set; }
        public virtual DbSet<FLPoints> FLPoints { get; set; }
        public virtual DbSet<RearFields> RearFields { get; set; }
        public virtual DbSet<Tokens> Tokens { get; set; }
        public virtual DbSet<BattlePonts> BattlePonts { get; set; }
        public virtual DbSet<DamageLog> DamageLog { get; set; }
        public virtual DbSet<Rooms> Rooms { get; set; }
        public virtual DbSet<TargetBlock> TargetBlock { get; set; }
        public virtual DbSet<UserDirect> UserDirect { get; set; }
        public virtual DbSet<ColInput> ColInput { get; set; }
        public virtual DbSet<PilotDirect> PilotDirect { get; set; }
        public virtual DbSet<VoteDirect> VoteDirect { get; set; }
        public virtual DbSet<PhaseGen> PhaseGen { get; set; }
        public virtual DbSet<PlaneSet> PlaneSet { get; set; }
        public virtual DbSet<PlanesOrders> PlanesOrders { get; set; }
        public virtual DbSet<BanList> BanList { get; set; }
        public virtual DbSet<Speech> Speech { get; set; }
        public virtual DbSet<ATCDispatcher> ATCDispatcher { get; set; }
        public virtual DbSet<ServiceArea> ServiceArea { get; set; }
        public virtual DbSet<SpeechPhrase> SpeechPhrase { get; set; }
        /// <summary>
        /// Таблица пилотов выразивших желание попадать в бан после смерти аватара
        /// </summary>
        public virtual DbSet<BanToKill> BanToKill { get; set; }
        public virtual DbSet<DurationBFL> DurationBFL { get; set; }
        public virtual DbSet<AreaCoord> AreaCoord { get; set; }
        public virtual DbSet<Area> Area { get; set; }
        public virtual DbSet<InfArea> InfArea { get; set; }
        public virtual DbSet<InfAreaCoord> InfAreaCoord { get; set; }
    }
}
