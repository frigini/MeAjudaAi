/**
 * Gera um link do WhatsApp para um número de telefone.
 * @param phone O número de telefone (com ou sem formatória).
 * @returns URL do WhatsApp ou null se o telefone for inválido.
 */
export function getWhatsappLink(phone: string): string | null {
    let cleanPhone = phone.replace(/\D/g, "");

    // If it starts with 55 and has enough digits to be DDI(2)+DDD(2)+Phone(8-9), assume DDI exists
    if (cleanPhone.startsWith("55") && cleanPhone.length >= 12) {
        cleanPhone = cleanPhone.substring(2);
    }

    // Validate: Brazilian phone should have at least 10 digits (DDD + number)
    return cleanPhone.length >= 10 ? `https://wa.me/55${cleanPhone}` : null;
}
