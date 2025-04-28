using Microsoft.AspNetCore.SignalR;

namespace e_learning.Hubs
{
    public class ChatHub : Hub
    {
        // 🛠️ ينضم لجروب الكورس لما يدخل
        public async Task JoinGroup(string courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Course_{courseId}");
        }

        // 🛠️ يخرج من الجروب لما يسيب الكورس
        public async Task LeaveGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Course_{courseId}");
        }

        // 🛠️ إرسال رسالة نصية أو مرفق
        public async Task SendMessage(string courseId, string userName, string message, string? attachmentUrl = null, int? replyToMessageId = null)
        {
            await Clients.Group($"Course_{courseId}").SendAsync("ReceiveMessage", new
            {
                UserName = userName,
                Text = message,
                AttachmentUrl = attachmentUrl,
                ReplyToMessageId = replyToMessageId,
                SentAt = DateTime.UtcNow
            });
        }

        // 🛠️ يبعث Typing Indicator لما المستخدم يكتب
        public async Task Typing(string courseId, string userName)
        {
            await Clients.Group($"Course_{courseId}").SendAsync("Typing", new
            {
                UserName = userName
            });
        }

        // 🛠️ لما المستخدم يعمل Seen للرسائل
        public async Task SeenMessages(string courseId, int userId)
        {
            await Clients.Group($"Course_{courseId}").SendAsync("MessagesSeen", new
            {
                UserId = userId,
                SeenAt = DateTime.UtcNow
            });
        }

        // 🛠️ لو المستخدم عدل رسالة
        public async Task EditMessage(string courseId, int messageId, string newText)
        {
            await Clients.Group($"Course_{courseId}").SendAsync("MessageEdited", new
            {
                MessageId = messageId,
                NewText = newText
            });
        }

        // 🛠️ لو المستخدم حذف رسالة
        public async Task DeleteMessage(string courseId, int messageId)
        {
            await Clients.Group($"Course_{courseId}").SendAsync("MessageDeleted", new
            {
                MessageId = messageId
            });
        }

        // 🛠️ لو المستخدم عمل Reaction (لايك أو قلب)
        public async Task ReactToMessage(string courseId, int messageId, string reactionType)
        {
            await Clients.Group($"Course_{courseId}").SendAsync("MessageReacted", new
            {
                MessageId = messageId,
                Reaction = reactionType
            });
        }
    }
}
