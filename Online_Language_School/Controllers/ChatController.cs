using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Online_Language_School.Data;
using Online_Language_School.ViewModels.Chat;
using Online_Language_School.Models;
using Microsoft.EntityFrameworkCore;

[Authorize] // доступ лише для автентифікованих користувачів
public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatController(ApplicationDbContext context,
                          UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 1 + 6: Список чатів відсортований за часом останнього повідомлення
    public async Task<IActionResult> Index()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return RedirectToAction("Login", "Account");

        var chatsRaw = await _context.ChatMessages
            .Where(m => m.SenderId == me.Id || m.ReceiverId == me.Id)
            .Select(m => new
            {
                PartnerId = m.SenderId == me.Id ? m.ReceiverId : m.SenderId,
                m.Content,
                m.SentAt
            })
            .GroupBy(x => x.PartnerId)
            .Select(g => new
            {
                PartnerId = g.Key,
                LastMessage = g.OrderByDescending(x => x.SentAt).First().Content,
                LastMessageTime = g.Max(x => x.SentAt)
            })
            .OrderByDescending(x => x.LastMessageTime)
            .ToListAsync();

        // підтягнути дані користувачів окремо
        var partnerIds = chatsRaw.Select(c => c.PartnerId).ToList();
        var partners = await _context.Users
            .Where(u => partnerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var chats = chatsRaw.Select(c => new ChatListItemViewModel
        {
            UserId = c.PartnerId,
            FullName = partners[c.PartnerId].FirstName + " " + partners[c.PartnerId].LastName,
            LastMessage = c.LastMessage,
            LastMessageTime = c.LastMessageTime
        }).ToList();

        return PartialView(chats);
    }

    // 2: Відкриття чату
    public async Task<IActionResult> Conversation(string id)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return RedirectToAction("Login", "Account");

        var messages = await _context.ChatMessages
            .Where(m =>
                (m.SenderId == me.Id && m.ReceiverId == id) ||
                (m.SenderId == id && m.ReceiverId == me.Id))
            .OrderByDescending(m => m.SentAt)
            .Select(m => new ChatMessageViewModel
            {
                IsMine = m.SenderId == me.Id,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToListAsync();

        ViewBag.User = await _context.Users.FindAsync(id);
        return PartialView(messages);
    }

    // 3: Надсилання повідомлення
    [HttpPost]
    public async Task<IActionResult> Send(string receiverId, string text)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Unauthorized();

        var msg = new ChatMessage
        {
            SenderId = me.Id,
            ReceiverId = receiverId,
            Content = text,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(msg);
        await _context.SaveChangesAsync();

        // Повертаємо оновлений список повідомлень
        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == me.Id && m.ReceiverId == receiverId) ||
                        (m.SenderId == receiverId && m.ReceiverId == me.Id))
            .OrderByDescending(m => m.SentAt)
            .Select(m => new ChatMessageViewModel
            {
                IsMine = m.SenderId == me.Id,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToListAsync();

        ViewBag.User = await _context.Users.FindAsync(receiverId);
        return PartialView("Conversation", messages);
    }


    // 5: Пошук користувача по ПІБ або @ID
    public async Task<IActionResult> Search(string query)
    {
        var me = await _userManager.GetUserAsync(User);

        if (string.IsNullOrWhiteSpace(query))
            return PartialView("SearchResult", new List<ApplicationUser>());

        IQueryable<ApplicationUser> users;

        if (query.StartsWith("@"))
        {
            var id = query.Substring(1);
            users = _context.Users.Where(x => x.Id.Contains(id));
        }
        else
        {
            users = _context.Users.Where(u => (u.FirstName + " " + u.LastName).Contains(query));
        }

        var result = await users
            .Where(u => u.Id != me.Id)
            .ToListAsync();

        return PartialView("SearchResult", result);
    }

}
