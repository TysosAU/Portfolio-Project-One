using Microsoft.EntityFrameworkCore;
using WebAPI.Db;
using WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Services
{
    public class HeroLoadoutService
    {
        private readonly DatabaseContext _context;

        public HeroLoadoutService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<HeroLoadout> CreateHeroLoadoutAsync(HeroLoadoutDTO heroLoadoutDTO)
        {
            var heroLoadout = new HeroLoadout
            {
                Commander = heroLoadoutDTO.Commander,
                Perk = heroLoadoutDTO.Perk,
                HeroOne = heroLoadoutDTO.HeroOne,
                HeroTwo = heroLoadoutDTO.HeroTwo,
                HeroThree = heroLoadoutDTO.HeroThree,
                HeroFour = heroLoadoutDTO.HeroFour,
                HeroFive = heroLoadoutDTO.HeroFive,
                GadgetOne = heroLoadoutDTO.GadgetOne,
                GadgetTwo = heroLoadoutDTO.GadgetTwo,
                CreatedBy = heroLoadoutDTO.CreatedBy
            };
            _context.HeroLoadouts.Add(heroLoadout);
            await _context.SaveChangesAsync();
            return heroLoadout;
        }

        public async Task<List<HeroLoadout>> GetAllHeroLoadoutsAsync()
        {
            if (_context.HeroLoadouts == null)
            {
                return null; 
            }
            return await _context.HeroLoadouts.ToListAsync();
        }

        public async Task<List<HeroLoadout>> GetHeroLoadoutsByCommanderAsync(string commander)
        {
            return await _context.HeroLoadouts
                .Where(h => h.Commander == commander)
                .ToListAsync();
        }

        public async Task<HeroLoadout> GetHeroLoadoutByIdAsync(int id)
        {
            return await _context.HeroLoadouts.FirstOrDefaultAsync(h => h.ID == id);
        }

        public async Task<HeroLoadout> UpdateHeroLoadoutAsync(HeroLoadout heroLoadout)
        {
            var existingHeroLoadout = await _context.HeroLoadouts.FirstOrDefaultAsync(h => h.ID == heroLoadout.ID);
            if (existingHeroLoadout == null) return null;
            existingHeroLoadout.Commander = heroLoadout.Commander;
            existingHeroLoadout.Perk = heroLoadout.Perk;
            existingHeroLoadout.HeroOne = heroLoadout.HeroOne;
            existingHeroLoadout.HeroTwo = heroLoadout.HeroTwo;
            existingHeroLoadout.HeroThree = heroLoadout.HeroThree;
            existingHeroLoadout.HeroFour = heroLoadout.HeroFour;
            existingHeroLoadout.HeroFive = heroLoadout.HeroFive;
            existingHeroLoadout.GadgetOne = heroLoadout.GadgetOne;
            existingHeroLoadout.GadgetTwo = heroLoadout.GadgetTwo;
            existingHeroLoadout.CreatedBy = heroLoadout.CreatedBy;
            await _context.SaveChangesAsync();
            return existingHeroLoadout;
        }

        public async Task<bool> DeleteHeroLoadoutAsync(int id)
        {
            var heroLoadout = await _context.HeroLoadouts.FirstOrDefaultAsync(h => h.ID == id);

            if (heroLoadout == null) return false;

            _context.HeroLoadouts.Remove(heroLoadout);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
