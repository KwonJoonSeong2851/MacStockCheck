using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Runtime.InteropServices;
using System.Media;


/// <summary>
/// 맥북 M1 에어 기본 모델 재고 확인 도우미입니다.
/// 제작: 맥쓰사 카페 SeongWall (kjoonseong2851@naver.com)
/// </summary>


namespace MacStockCheck
{
    class main
    {
        [DllImport("kernel32.dll")]
        extern public static void Beep(int freq, int dur);

        const string grayURL = "https://www.apple.com/kr-k12/shop/buy-mac/macbook-air/%EC%8A%A4%ED%8E%98%EC%9D%B4%EC%8A%A4-%EA%B7%B8%EB%A0%88%EC%9D%B4-apple-m1-%EC%B9%A9(8%EC%BD%94%EC%96%B4-cpu-%EB%B0%8F-7%EC%BD%94%EC%96%B4-gpu)-256gb#";
        const string goldURL = "https://www.apple.com/kr-k12/shop/buy-mac/macbook-air/%EA%B3%A8%EB%93%9C-apple-m1-%EC%B9%A9(8%EC%BD%94%EC%96%B4-cpu-%EB%B0%8F-7%EC%BD%94%EC%96%B4-gpu)-256gb#";
        const string silverURL = "https://www.apple.com/kr-k12/shop/buy-mac/macbook-air/%EC%8B%A4%EB%B2%84-apple-m1-%EC%B9%A9(8%EC%BD%94%EC%96%B4-cpu-%EB%B0%8F-7%EC%BD%94%EC%96%B4-gpu)-256gb#";

        public static bool running;
        public static string curURL;
        public static SoundPlayer sp;

        static void Main(string[] args)
        {

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("애플스토어 맥북 에어 기본 모델 픽업 재고확인 도우미 입니다.");
                Console.WriteLine("색상 번호를 입력후 엔터를 눌러 주세요.");
                Console.WriteLine("1. 스페이스 그레이");
                Console.WriteLine("2. 실버");
                Console.WriteLine("3. 골드");

                bool select = false;

                while (!select)
                {
                    switch (Console.ReadLine())
                    {
                        case "1":
                            Console.WriteLine("스페이스 그레이 색상을 선택하셨습니다.");
                            curURL = grayURL;
                            select = true;
                            break;

                        case "2":
                            Console.WriteLine("실버 색상을 선택하셨습니다.");
                            curURL = silverURL;
                            select = true;
                            break;

                        case "3":
                            Console.WriteLine("골드 색상을 선택하셨습니다.");
                            curURL = goldURL;
                            select = true;
                            break;

                        default:
                            Console.WriteLine("단일 숫자만 입력후 엔터를 입력해 주세요. ex) 1");
                            break;
                    }
                }


                Thread parsingThread = new Thread(() => Parsing());

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("1. 시작");
                Console.WriteLine("2. 종료");
                Console.WriteLine("메뉴 선택후 엔터를 입력해주세요. ex) 1 -> 엔터");

                while (true)
                {
                    switch (Console.ReadLine())
                    {
                        case "1":
                            sp = new SoundPlayer("alarm.wav");
                            Console.WriteLine("재고 확인을 준비합니다. (5초 정도 소요)");
                            running = true;
                            parsingThread.Start();
                            break;

                        case "2":
                            Console.WriteLine("재고 확인을 정지합니다.");
                            running = false;
                            parsingThread.Join();
                            return;

                        default:
                            Console.WriteLine("입력한 값을 확인해 주세요.");
                            break;
                    }
                }
            
        }

        static void Parsing()
        {

            
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            options.AddArgument("--headless");
            options.AddArgument("user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

            IWebDriver driver = new ChromeDriver(driverService, options);


            driver.Navigate().GoToUrl(curURL);

            Console.WriteLine("재고 확인을 시작합니다.");
            string resultText;
            while (running)
            {
                try
                {
                    driver.Navigate().GoToUrl(curURL);
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                }
                catch(Exception)
                {
                    Console.WriteLine("예기치 못한 오류가 발생하였습니다. 오류코드:A");
                    Console.ReadKey();
                    break;
                }

                DateTime time = DateTime.Now;
                try
                {
                    resultText = driver.FindElement(By.ClassName("as-retailavailabilitytrigger-value")).Text;

                    Console.WriteLine("[" + time.ToLongTimeString() + "] 상태:" + resultText);

                    if (resultText.Contains("위치")) 
                    {
                        sp.Play();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("[" + time.ToLongTimeString() + "] 상태: 예기치 못한 오류가 발생하였습니다. 오류코드:B");
                    Console.ReadKey();
                    break;
                }

                Thread.Sleep(3000);

            }

            driver.Close();
        }

    }
}
