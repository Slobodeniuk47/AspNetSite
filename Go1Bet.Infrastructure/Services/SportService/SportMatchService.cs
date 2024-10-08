﻿using AutoMapper;
using Go1Bet.Core.Context;
using Go1Bet.Core.Entities.Sport;
using Go1Bet.Infrastructure.DTO_s.Sport.Opponent;
using Go1Bet.Infrastructure.DTO_s.Sport.Person;
using Go1Bet.Infrastructure.DTO_s.Sport.Odd;
using Go1Bet.Infrastructure.DTO_s.Sport.SportEvent;
using Go1Bet.Infrastructure.DTO_s.Sport.SportMatch;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Paddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Go1Bet.Infrastructure.DTO_s.Sport.Bet;

namespace Go1Bet.Infrastructure.Services.SportService
{
    public class SportMatchService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        public SportMatchService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ServiceResponse> GetAllAsync()
        {
            try
            {
                var sportEvents = await _context.SportMatches

                    .Select(sm => new SportMatchItemDTO
                    {
                        Id = sm.Id,
                        Name = sm.Name,
                        DateCreated = sm.DateCreated,
                        DateEnd = sm.DateEnd,
                        DateStart = sm.DateStart,
                        BettingFund = sm.Odds.Sum(x => x.Bets.Sum(b => b.Amount)),
                        CountBets = sm.Odds.Sum(x => x.Bets.Count()),
                        SportEventId = sm.SportEventId,
                        SportEventName = sm.SportEvent.Name,
                    }).ToListAsync();

                return new ServiceResponse
                {
                    Success = true,
                    Payload = sportEvents
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ServiceResponse> GetByIdAsync(string id)
        {
            try
            {
                var sportEvents = await _context.SportMatches
                    .Where(p => p.Id == id)
                    .Select(sm => new SportMatchItemDTO
                    {
                        Id = sm.Id,
                        Name = sm.Name,
                        DateCreated = sm.DateCreated,
                        DateEnd = sm.DateEnd,
                        DateStart = sm.DateStart,
                        BettingFund = sm.Odds.Sum(x=> x.Bets.Sum(b=>b.Amount)),
                        CountBets = sm.Odds.Sum(x=>x.Bets.Count()),
                        Opponents = sm.Opponents.Where(o=> o.SportMatchId == sm.Id)
                        .Select(o => new OpponentItemDTO 
                        { 
                            Id = o.Id, Name = o.Name, SportMatchId = o.SportMatchId, DateCreated = o.DateCreated, countTeammates = o.Teammates.Count() 
                        }).ToList(),
                        //Odds
                        Odds = sm.Odds.Where(o=> o.SportMatchId == sm.Id)
                        .Select(odds => new OddItemDTO 
                        { 
                            Id = odds.Id, Name = odds.Name, OpponentId = odds.OpponentId, SportMatchId = odds.SportMatchId, Type = odds.Type, Value = odds.Value,
                            Bets = odds.Bets.Where(b => b.OddId == odds.Id).Select(b => new BetItemDTO { Id = b.Id, Amount = b.Amount, BetTime = b.BetTime, OddId = b.OddId, UserId = b.UserId }).ToList(),
                            CountBets = odds.Bets.Count(), 
                            BettingFund = odds.Bets.Sum(b => b.Amount)
                        }).ToList(),
                        
                        SportEventId = sm.SportEventId,
                        SportEventName = sm.SportEvent.Name,
                        SportEvent = new SportEventItemDTO
                        {
                            Id = sm.SportEvent.Id,
                            Name = sm.SportEvent.Name,
                            Description = sm.SportEvent.Description,
                            DateCreated = sm.SportEvent.DateCreated,
                            DateStart = sm.SportEvent.DateStart,
                            DateEnd = sm.SportEvent.DateEnd,
                            
                        }
                    }).ToListAsync();

                return new ServiceResponse
                {
                    Success = true,
                    Payload = sportEvents
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ServiceResponse> CreateAsync(SportMatchCreateDTO model)
        {
            var sportEvent = _mapper.Map<SportMatchEntity>(model);
            await _context.SportMatches.AddAsync(sportEvent);
            await _context.SaveChangesAsync();
            return new ServiceResponse
            {
                Message = "Sport Match was created",
                Success = true,
            };
        }
        public async Task<ServiceResponse> EditAsync(SportMatchEditDTO model)
        {
            var oldSportMatch = await _context.SportMatches.Where(x => x.Id == model.Id).FirstOrDefaultAsync();

            if (oldSportMatch == null)
            {
                return new ServiceResponse()
                {
                    Message = "Upload sport match is not correct, upload is closed",
                    Success = false,
                };
            }
            var newSportMatch = _mapper.Map<SportMatchEntity>(model);

            _context.SportMatches.Update(newSportMatch);
            await _context.SaveChangesAsync();

            return new ServiceResponse()
            {
                Message = "Sport Match update was successful",
                Success = true,
            };

        }
        public async Task<ServiceResponse> DeleteAsync(string id)
        {
            var sportMatch = await _context.SportMatches.SingleOrDefaultAsync(x => x.Id == id);
            if (sportMatch == null)
            {
                return new ServiceResponse()
                {
                    Message = "Uploaded sport match is not correct, uploaded is closed",
                    Success = false,
                };
            }



            _context.SportMatches.Remove(sportMatch);
            await _context.SaveChangesAsync();
            return new ServiceResponse()
            {
                Message = "Sport Match has been deleted",
                Success = true,
            };
        }
    }
}
