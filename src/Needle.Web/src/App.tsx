import { HomeFeaturesSection } from './components/HomeFeaturesSection';
import { HeroSection } from './components/HeroSection';
import { AlbumSearchPreview } from './components/AlbumSearchPreview';
import './App.css';

function App() {
    return (
        <main className="app-shell">
            <HeroSection />
            <AlbumSearchPreview />
            <HomeFeaturesSection />
        </main>
    );
}

export default App;


