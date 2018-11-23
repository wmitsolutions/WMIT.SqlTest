create function fail(@message varchar(max)) as
begin
	throw 60000, @message, 1
end
go


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
	if 1=2 throw 60000, 'das und das nicht richtig', 1
	if 1=1 throw 60000, 'etwas anderes nicht richtig', 1
go



drop procedure dbo.TestProc3
go
create procedure dbo.TestProc3
as
	throw 51000, 'my err', 1
	return 0
go
