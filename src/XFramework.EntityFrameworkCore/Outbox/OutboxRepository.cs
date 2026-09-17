using Microsoft.EntityFrameworkCore;
using XFramework.EntityFrameworkCore.Persistence;
namespace XFramework.EntityFrameworkCore.Outbox;
public sealed class OutboxRepository(XFrameworkDbContext db):IOutboxRepository
{
 public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize,string lockId,DateTime now,DateTime lockedUntil,CancellationToken ct=default){var rows=await db.OutboxMessages.Where(x=>(x.Status==OutboxMessageStatus.Pending || (x.Status==OutboxMessageStatus.Processing && x.LockedUntilUtc<now)) && (x.NextAttemptOnUtc==null||x.NextAttemptOnUtc<=now)).OrderBy(x=>x.CreatedOnUtc).Take(batchSize).ToListAsync(ct);foreach(var x in rows){x.Status=OutboxMessageStatus.Processing;x.LockId=lockId;x.LockedUntilUtc=lockedUntil;}await db.SaveChangesAsync(ct);return rows;}
 public async Task MarkCompletedAsync(Guid id,string lockId,DateTime completed,CancellationToken ct=default){var x=await db.OutboxMessages.SingleOrDefaultAsync(x=>x.Id==id&&x.LockId==lockId,ct);if(x is null)return;x.Status=OutboxMessageStatus.Completed;x.ProcessedOnUtc=completed;x.LockId=null;x.LockedUntilUtc=null;await db.SaveChangesAsync(ct);}
 public async Task MarkFailedAsync(Guid id,string lockId,DateTime next,string error,CancellationToken ct=default){var x=await db.OutboxMessages.SingleOrDefaultAsync(x=>x.Id==id&&x.LockId==lockId,ct);if(x is null)return;x.Status=OutboxMessageStatus.Pending;x.RetryCount++;x.NextAttemptOnUtc=next;x.LastError=error;x.LockId=null;x.LockedUntilUtc=null;await db.SaveChangesAsync(ct);}
 public async Task ReleaseExpiredLeasesAsync(DateTime now,CancellationToken ct=default){var rows=await db.OutboxMessages.Where(x=>x.Status==OutboxMessageStatus.Processing&&x.LockedUntilUtc<now).ToListAsync(ct);foreach(var x in rows){x.Status=OutboxMessageStatus.Pending;x.LockId=null;x.LockedUntilUtc=null;}if(rows.Count>0)await db.SaveChangesAsync(ct);}
}
