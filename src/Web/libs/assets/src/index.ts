export const LOGOS_PATH = '/logos/';

export const logos = {
  iconAzul: `${LOGOS_PATH}logo-icon-azul.png`,
  textAzul: `${LOGOS_PATH}logo-text-azul.png`,
  iconBranco: `${LOGOS_PATH}logo-icon-branco.png`,
  textBranco: `${LOGOS_PATH}logo-text-branco.png`,
} as const;

export type LogoName = keyof typeof logos;
