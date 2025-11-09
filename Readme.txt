Basic - > TitleBar: Custom titleBar sử dụng MVVM
Basic -> TabControl: cách tạo ra một TabControl sử dụng MVVM


historyHerculesSimulation:
Rev1: chứa titleBar + TabControl. 
Khi có 10 tab, nếu làm theo cách hiện tại, TabControlViewModel sẽ khởi tạo cùng lúc 10 ViewModel con (HomeViewModel, SettingViewModel, SerialViewModel, TcpClientViewModel, v.v.).

Vấn đề: Nếu các ViewModel con này thực hiện các tác vụ "nặng" (như kết nối CSDL, gọi API, khởi tạo dịch vụ...) ngay trong hàm khởi tạo (constructor), ứng dụng sẽ khởi động RẤT CHẬM và tốn nhiều RAM. Đây gọi là "Eager Loading" (tải "háo hức").

Giải pháp: dùng "Lazy Loading" (tải "lười"). chỉ tạo ContentViewModel (ví dụ: HomeViewModel) khi người dùng thực sự nhấn vào tab đó lần đầu tiên. -> Improve on Rev2