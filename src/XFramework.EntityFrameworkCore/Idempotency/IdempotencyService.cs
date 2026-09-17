using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.EntityFrameworkCore.Persistence;
namespace XFramework.EntityFrameworkCore.Idempotency;
public sealed class IdempotencyService(XFrameworkDbContext db):IIdempotencyService
{
 public async Task<bool> TryBeginProcessingAsync(Guid eventId,string handlerName,string? correlationId=null,CancellationToken ct=default)
 {
   var exists=await db.ProcessedMessages.AnyAsync(x=>x.EventId==eventId&&x.HandlerName==handlerName,ct); if(exists)return false;
   db.ProcessedMessages.Add(new ProcessedMessage{EventId=eventId,HandlerName=handlerName,ProcessedOnUtc=DateTime.UtcNow,CorrelationId=correlationId}); return true;
 }
}
