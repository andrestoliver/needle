import { useState } from 'react';

export function AlbumSearchPreview() {
    const [query, setQuery] = useState('');

    return (
        <section className="search-preview" aria-labelledby="search-preview-title">
            <h2 id="search-preview-title">Busque seu próximo álbum</h2>

            <label htmlFor="album-search">Nome do álbum</label>
            <input
                id="album-search"
                type="search"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Ex: Kind of Blue"
            />

            {query.trim() !== '' && (
                <p>
                    Você está buscando por: <strong>{query}</strong>
                </p>
            )}
        </section>
    );
}