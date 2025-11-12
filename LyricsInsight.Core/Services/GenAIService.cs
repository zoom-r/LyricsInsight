// Файл: LyricsInsight.Core/Services/GenAiService.cs

using System;
using System.Threading.Tasks;
using Google.GenAI; // Новият SDK
using Google.GenAI.Types;

namespace LyricsInsight.Core.Services
{
    public class GenAiService
    {
        private readonly Client _geminiModel;

        public GenAiService()
        {
            // 1. Прочитаме ключа от променливата на средата
            var apiKey = System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Хвърляме грешка, ако ключът не е настроен
                throw new InvalidOperationException(
                    "API ключът за Google GenAI не е намерен. " +
                    "Моля, задайте 'GOOGLE_API_KEY' като променлива на средата.");
            }

            // 2. Инициализираме модела.
            // "gemini-1.5-flash" е най-бързият и е идеален за тази задача.
            _geminiModel = new Client(apiKey:apiKey);
        }

        // 3. Асинхронен метод, който нашият ViewModel ще вика
        public async Task<(string, bool)> GenerateAnalysisAsync(string lyrics, string song, string artist, string album)
        {
            // 4. Това е Prompt-ът! 
            // Тук казваме на AI какво точно искаме, на български.
            var prompt = $@"Ти си експертен музикален анализатор. Анализирай следната песен, използвайки текста на песента и информация, която имаш от интернет. 
            Отговори директно с анализа. Формулирай го професионално.
            Фокусирай се върху:
            1. Основната тема и значение.
            2. Емоциите и настроението.
            3. Всички забележителни метафори или образи.
            4. Дали има референции към други хора, песни...
            5. Мястото на песента в албума (ако участва в албум - не говорим за това, ако е публикувана само като single)
            Отговори на български език.
            
            Име на песента: {song}
            Име на артиста: {artist}
            Име на Албума/EP-то/Single-а: {album}
            Текст на песента:
            ---
            {lyrics}
            ---

            Твоят анализ:";

            try
            {
                var response = await _geminiModel.Models.GenerateContentAsync(model:"gemini-2.5-flash", contents:prompt);
                
                // Проверяваме дали има текст в отговора
                if (response.Candidates.Any() && response.Candidates.First().Content.Parts.Any())
                {
                    return (response.Candidates.First().Content.Parts.First().Text, true);
                }
                
                return ("AI моделът не върна отговор.", false);
            }
            catch (Exception ex)
            {
                if (ex is ServerError)
                    return ("Има проблем със сървърите на Google GenAI. Моля, опитайте отново по-късно.", false);
                // Връщаме съобщението за грешка, за да го видим в UI-я
                return ($"Грешка при извикването на Google GenAI: {ex.Message}", false);
            }
        }
    }
}