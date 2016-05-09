
netsh http add urlacl url=http://+:80/ user="NT AUTHORITY\NETWORK SERVICE"

src\REstate.Services.Auth\bin\Release\REstate.Services.Auth.exe install
src\REstate.Services.Core\bin\Release\REstate.Services.Core.exe install
src\REstate.Services.Chrono\bin\Release\REstate.Services.Chrono.exe install
src\REstate.Services.ChronoConsumer\bin\Release\REstate.Services.ChronoCnsumer.exe install
src\REstate.Service.AdminUI\bin\Release\REstate.Services.AdminUI.exe install