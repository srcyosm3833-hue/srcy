namespace Zn.ClientWebApi.Audit
{
    /// <summary>
    /// Audit saklama süresi (retention) yapılandırması. <c>Audit:Retention</c> bölümünden bind edilir.
    /// <para>
    /// Tüm süreler GÜN cinsindendir; <c>SweepIntervalHours</c> SAAT cinsindendir. Bir süre 0 veya
    /// negatifse o tür için temizlik ATLANIR (devre dışı). Blog ve Message için süresi geçmiş
    /// kayıtların IP hash alanı null'a çekilir (anonimleştirme — kayıt korunur); SearchLog için
    /// süresi geçmiş satırlar tamamen silinir.
    /// </para>
    /// </summary>
    public sealed class AuditRetentionOptions
    {
        /// <summary>Yapılandırma bölümü adı: <c>Audit:Retention</c>.</summary>
        public const string SectionName = "Audit:Retention";

        /// <summary>
        /// Blog <c>CreatorIpHash</c> saklama süresi (gün). Bu süreden eski bloglarda IP hash null'a
        /// çekilir. 0/negatif → Blog IP temizliği atlanır.
        /// </summary>
        public int BlogIpDays { get; set; } = 180;

        /// <summary>
        /// Message <c>SenderIpHash</c> saklama süresi (gün). Bu süreden eski mesajlarda IP hash
        /// null'a çekilir. 0/negatif → Message IP temizliği atlanır.
        /// </summary>
        public int MessageIpDays { get; set; } = 180;

        /// <summary>
        /// SearchLog saklama süresi (gün). Bu süreden eski arama log satırları SİLİNİR.
        /// 0/negatif → SearchLog temizliği atlanır.
        /// </summary>
        public int SearchLogDays { get; set; } = 90;

        /// <summary>
        /// Temizlik taramasının çalışma aralığı (saat). 0/negatif → güvenli varsayılana (24 saat)
        /// düşülür; background service hiçbir zaman sıfır/negatif gecikmeyle dönmez.
        /// </summary>
        public int SweepIntervalHours { get; set; } = 24;
    }
}
