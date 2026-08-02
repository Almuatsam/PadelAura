import { useTranslation } from "react-i18next"

/** Shared by CustomerLayout and AdminLayout — both previously hand-rolled this identically. */
export function useLanguageToggle(): () => void {
  const { i18n } = useTranslation()

  return () => {
    void i18n.changeLanguage(i18n.language.startsWith("ar") ? "en" : "ar")
  }
}
