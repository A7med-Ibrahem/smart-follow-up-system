using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class NoteService
    {
        private readonly AppDbContext _context;

        public NoteService(AppDbContext context)
        {
            _context = context;
        }

        // Add Note
        public async Task<NoteResponseDto?> AddNoteAsync(CreateNoteRequestDto request, long doctorId)
        {
            var existingCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.Id == request.CaseId && c.DoctorId == doctorId);

            if (existingCase == null) return null;

            var note = new DoctorNote
            {
                CaseId = request.CaseId,
                DoctorId = doctorId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.DoctorNotes.Add(note);
            await _context.SaveChangesAsync();

            var doctor = await _context.Users.FindAsync(doctorId);

            return new NoteResponseDto
            {
                Id = note.Id,
                CaseId = note.CaseId,
                DoctorName = doctor?.Name ?? "",
                Content = note.Content,
                CreatedAt = note.CreatedAt
            };
        }

        // Get Notes for Case
        public async Task<List<NoteResponseDto>> GetCaseNotesAsync(long caseId)
        {
            return await _context.DoctorNotes
                .Where(n => n.CaseId == caseId)
                .Include(n => n.Doctor)
                .Select(n => new NoteResponseDto
                {
                    Id = n.Id,
                    CaseId = n.CaseId,
                    DoctorName = n.Doctor.Name,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}