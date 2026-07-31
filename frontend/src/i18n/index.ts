import i18n from "i18next"
import { initReactI18next } from "react-i18next"
import LanguageDetector from "i18next-browser-languagedetector"
import en from "./locales/en.json"
import ar from "./locales/ar.json"

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: { en, ar },
    fallbackLng: "en",
    supportedLngs: ["en", "ar"],
    interpolation: { escapeValue: false },
  })

function applyDirection(language: string) {
  const direction = language.startsWith("ar") ? "rtl" : "ltr"
  document.documentElement.dir = direction
  document.documentElement.lang = language
}

applyDirection(i18n.resolvedLanguage ?? i18n.language)
i18n.on("languageChanged", applyDirection)

export default i18n
