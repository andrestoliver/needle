import './App.css';

function App() {
    return (
        <main className="app-shell">
            <section className="hero">
                <p className="eyebrow">Needle</p>
                <h1>Seu diário de álbuns</h1>
                <p className="hero-description">
                    Busque álbuns, importe favoritos para sua coleção e registre reviews
                    com notas de 0.5 a 5.0.
                </p>
            </section>

            <section className="next-steps" aria-labelledby="next-steps-title">
                <h2 id="next-steps-title">Primeiros fluxos</h2>

                <div className="cards">
                    <article className="card">
                        <span>01</span>
                        <h3>Pesquisar álbuns</h3>
                        <p>Encontrar álbuns no catálogo externo do MusicBrainz.</p>
                    </article>

                    <article className="card">
                        <span>02</span>
                        <h3>Importar favoritos</h3>
                        <p>Trazer álbuns escolhidos para o catálogo local do Needle.</p>
                    </article>

                    <article className="card">
                        <span>03</span>
                        <h3>Registrar reviews</h3>
                        <p>Avaliar álbuns e acompanhar suas impressões musicais.</p>
                    </article>
                </div>
            </section>
        </main>
    );
}

export default App;
