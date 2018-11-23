drop procedure dbo.TestProc
go
create procedure dbo.TestProc 
as
	return 0
go

drop procedure dbo.TestProc2
go
create procedure dbo.TestProc2
as
	return 1
go

drop procedure dbo.TestProc3
go
create procedure dbo.TestProc3
as
	throw 51000, 'my err', 1
	return 0
go
