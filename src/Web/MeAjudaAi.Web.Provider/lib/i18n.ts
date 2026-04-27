import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import Backend from 'i18next-http-backend';

i18n
  .use(Backend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'pt',
    supportedLngs: ['pt', 'en'],
    debug: false,
    interpolation: {
      escapeValue: false,
    },
    load: 'languageOnly',
    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
    },
    detection: {
      order: ['cookie'],
      caches: ['cookie'],
    },
    react: {
      useSuspense: false,
    },
    resources: {
      pt: {
        common: {
          loading: "Carregando...",
          error: "Ocorreu um erro",
          welcome: "Bem-vindo ao Portal do Prestador",
          dashboard: "Painel",
          services: "Serviços",
          schedule: "Agenda",
          profile: "Perfil"
        }
      },
      en: {
        common: {
          loading: "Loading...",
          error: "An error occurred",
          welcome: "Welcome to the Provider Portal",
          dashboard: "Dashboard",
          services: "Services",
          schedule: "Schedule",
          profile: "Profile"
        }
      }
    }
  });

export default i18n;
