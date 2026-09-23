# Outbox Lease Expiration Race — V43

Hardened lease renewal so an expired worker cannot renew a lease it no longer owns.

RenewLeaseAsync now receives `nowUtc` and requires `LockedUntilUtc > nowUtc` in addition to matching message id, lock id, and Processing status.

Added SQL Server integration coverage for an expired worker attempting renewal.
