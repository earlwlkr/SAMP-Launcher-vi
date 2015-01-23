# SAMP-Launcher-vi
GTA San Andreas Multiplayer launcher for specific server

## Compile

* Tải và cài đặt Visual Studio (IDE dùng để lập trình ứng dụng Windows). Có thể tải bản miễn phí Visual Studio Express 2013 ở [đây](https://app.vssps.visualstudio.com/profile/review?download=true&family=VisualStudioExpressDesktop&release=VisualStudio2013Upd4&type=web&slcid=0x409&context=eyJwZSI6MSwicGMiOjEsImljIjoxLCJhbyI6MSwiYW0iOjAsIm9wIjpudWxsLCJhZCI6bnVsbCwiZmEiOjAsImF1IjpudWxsLCJjdiI6MzEwMjg4MDEyLCJmcyI6MCwic3UiOjAsImVyIjoxfQ2)
* Dùng Visual Studio mở file *SAMP-Launcher.sln*
* Mã nguồn xử lý chính sẽ nằm trong file *MainWindow.xaml.cs*
* Sửa các biến `_serverName`, `_serverPort` theo thông tin server của bạn
* <kbd>CTRL</kbd>+<kbd>F5</kbd> để compile thành tập tin exe, file xuất ra sẽ nằm trong thư mục \bin