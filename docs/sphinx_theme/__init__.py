"""IdentityServer Sphinx Theme."""

import os

from docutils import nodes

__version__ = '1.0.0'


def get_html_theme_path():
    """Return the path to the theme directory."""
    return os.path.dirname(os.path.abspath(__file__))


def _remove_document_title(app, doctree, docname):
    """Strip the top-level section title from the HTML body.

    In MkDocs the page title is rendered exclusively by the layout template
    header; it never appears inside the content body.  Sphinx, by contrast,
    emits the RST document title as an <h1> inside the body.  This transform
    removes that title node so the Sphinx output matches MkDocs parity: the
    first visible heading in the body is an <h2> (RST sub-section level).

    env.titles[docname] is populated during the read phase, before this event
    fires, so the template {{ title }} variable, navigation, breadcrumbs and
    search index are all unaffected.
    """
    for node in doctree.children:
        if isinstance(node, nodes.section):
            for child in node.children[:]:
                if isinstance(child, nodes.title):
                    node.remove(child)
                    break
            break


def setup(app):
    """Register the theme with Sphinx."""
    app.add_html_theme(
        'identityserver_sphinx',
        os.path.dirname(os.path.abspath(__file__))
    )
    app.connect('doctree-resolved', _remove_document_title)
    return {
        'version': __version__,
        'parallel_read_safe': True,
    }
