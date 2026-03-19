import { ProfileHeader } from "../components/profile/profile-header";
import { ProfileDescription } from "../components/profile/profile-description";
import { ProfileServices } from "../components/profile/profile-services";
import { ProfileReviews } from "../components/profile/profile-reviews";

// Mock Data
const PROFILE_MOCK = {
  name: "José",
  email: "emailgrandedocara@gmail.com",
  isOnline: true, // Toggle this to see the offline state (Imagem 3)
  phones: ["(00) 0 0000 - 0000", "(00) 0 0000 - 0000", "(00) 0 0000 - 0000"],
  rating: 3.5,
  description: "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
  services: ["Serviço com nome grande", "Serviço 1", "Serviço 2", "Serviço 3", "Serviço 4", "Serviço 5"],
  reviews: Array.from({ length: 4 }).map((_, i) => ({
    id: `rev-${i}`,
    rating: 3,
    text: "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
    author: "Wesley Veiga",
    date: "15/02/2024",
  }))
};

export default function Index() {
  return (
    <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6 lg:px-8">
      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10">
        <ProfileHeader
          name={PROFILE_MOCK.name}
          email={PROFILE_MOCK.email}
          isOnline={PROFILE_MOCK.isOnline}
          phones={PROFILE_MOCK.phones}
          rating={PROFILE_MOCK.rating}
        />
        
        <ProfileDescription description={PROFILE_MOCK.description} />
        
        <ProfileServices services={PROFILE_MOCK.services} />

        <ProfileReviews reviews={PROFILE_MOCK.reviews} />
      </main>
    </div>
  );
}
